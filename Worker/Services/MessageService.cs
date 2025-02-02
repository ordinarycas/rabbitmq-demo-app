using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.WebSockets;
using System.Threading;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

// MessageService.cs
namespace Worker.Services
{
    public class MessageService
    {
        private readonly WebSocketService _webSocketService;
        private readonly ILogger<MessageService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _rabbitMqHostName;
        private readonly string _queueName;
        private static List<(string message, bool isSent)> _messageQueue = new List<(string message, bool isSent)>();
        private IConnection? _rabbitMqConnection;
        private IModel? _rabbitMqChannel;
        private bool _isListening = false;
        // 假設每隔一段時間檢查一次 MQ 是否有資料，並根據 WebSocket 連線數清除 _messageQueue
        private Timer? _clearMessageQueueTimer;

        public MessageService(WebSocketService webSocketService, ILogger<MessageService> logger, IConfiguration configuration)
        {
            _webSocketService = webSocketService;
            _logger = logger;  // 将 logger 注入到 _logger 变量
            _configuration = configuration;

            // 讀取設定檔中的 RabbitMQ 設定
            _rabbitMqHostName = _configuration["RabbitMQ:HostName"] ?? "localhost";
            _queueName = _configuration["RabbitMQ:QueueName"] ?? "task_queue";
        }

        // 開始監聽 RabbitMQ 訊息
        public void StartListening()
        {
            // 确保 WebSocket 已连接，若没有连接则不启动 RabbitMQ 消费者
            /*
            if (_webSocketService.GetCurrentConnectionCount() == 0)
            {
                _logger.LogInformation("No WebSocket connections. RabbitMQ listener will not be started.");
                return; // 不启动 RabbitMQ 消费者
            }
            */

            if (_isListening)
            {
                _logger.LogInformation("RabbitMQ consumer is already listening.");
                return; // 防止重複啟動監聽
            }

            var factory = new ConnectionFactory() { HostName = _rabbitMqHostName };
            _rabbitMqConnection = factory.CreateConnection();
            _rabbitMqChannel = _rabbitMqConnection.CreateModel();

            _rabbitMqChannel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(_rabbitMqChannel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // 儲存訊息到緩存中，無論 WebSocket 是否已連線
                _logger.LogInformation($"Received message from RabbitMQ. Message: {message}");
                _messageQueue.Add((message, false));

                // 如果 WebSocket 已連線，則立即發送訊息到所有連線
                if (_webSocketService.GetCurrentConnectionCount() > 0)
                {
                    BroadcastMessageToWebSockets(message);  // 發送訊息給所有 WebSocket 連線
                }
                else
                {
                    // 若 WebSocket 沒有連線，等待連線後再發送訊息
                    _logger.LogInformation("WebSocket is not connected, message cached.");
                    // 註：這裡不再停止 RabbitMQ 的監聽，即使 WebSocket 斷開
                    //StopListening();
                }
            };

            _rabbitMqChannel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
            _isListening = true;
            _logger.LogInformation("Started listening to RabbitMQ.");
            // 設置定時器，每10秒檢查一次 MQ 和 WebSocket 狀態
            _clearMessageQueueTimer = new Timer(ClearMessageQueueIfEmpty, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

        }

        // 檢查 MQ 是否無資料，並根據 WebSocket 連線數清除 _messageQueue
        private void ClearMessageQueueIfEmpty(object? state)
        {
            int connectionCount = _webSocketService.GetCurrentConnectionCount();

            if (connectionCount == 0)
            {
                // 沒有 WebSocket 連線時，清空 _messageQueue
                _logger.LogInformation("No WebSocket connections, clearing message queue.");
                _messageQueue.Clear(); // 清空訊息緩存
            }
            else
            {
                // 有 WebSocket 連線時，只清除已標記為已發送的訊息
                _logger.LogInformation("WebSocket connections exist, clearing sent messages.");
                _messageQueue.RemoveAll(msg => msg.isSent); // 清除已發送的訊息
            }
        }


        // 停止監聽 RabbitMQ 訊息
        public void StopListening()
        {
            if (!_isListening)
            {
                _logger.LogInformation("RabbitMQ consumer is not listening.");
                return; // 防止重複停止監聽
            }

            // 檢查是否已經關閉通道
            if (_rabbitMqChannel != null && _rabbitMqChannel.IsOpen)
            {
                _rabbitMqChannel?.Close();
            }

            if (_rabbitMqConnection != null && _rabbitMqConnection.IsOpen)
            {
                _rabbitMqConnection?.Close();
            }

            _isListening = false;
            _logger.LogInformation("Stopped listening to RabbitMQ.");

            // 停止監聽時清除緩存的訊息
            _messageQueue.Clear();
            _clearMessageQueueTimer?.Dispose(); // 停止定時器
        }

        // 判断是否需要停止监听 RabbitMQ，根据 WebSocket 连接数量
        public void CheckStopListening()
        {
            int connectionCount = _webSocketService.GetCurrentConnectionCount();

            if (connectionCount == 0 && _isListening)
            {
                // 如果沒有 WebSocket 連線，且正在監聽 RabbitMQ，停止監聽
                _logger.LogInformation("No WebSocket connections, stopping RabbitMQ listener.");
                StopListening();
            }
            else if (connectionCount > 0 && !_isListening)
            {
                // 如果有 WebSocket 連線，且目前沒有在監聽 RabbitMQ，則啟動監聽
                _logger.LogInformation("WebSocket connections exist, starting RabbitMQ listener.");
                StartListening();
            }
        }


        // 将消息广播到所有 WebSocket 客户端
        public async Task BroadcastMessageToWebSockets(string message)
        {
            // _logger.LogInformation($"Broadcasting message to WebSockets: {message}");
            
            var messageObject = new
            {
                type = "message",
                content = message
            };
            
            string jsonMessage = JsonConvert.SerializeObject(messageObject);
            var allConnections = _webSocketService.GetAllWebSockets();

            // 发送给第一个成功的 WebSocket 后标记消息为已发送
            bool messageSent = false;

            foreach (var webSocket in allConnections)
            {
                if (webSocket?.State == WebSocketState.Open)
                {
                    await SendMessageToWebSocket(webSocket, jsonMessage);

                    // 一旦消息成功发送给一个 WebSocket，就标记该消息为已发送
                    if (!messageSent)
                    {
                        // 标记消息为已发送并清除
                        _messageQueue.RemoveAll(msg => msg.message == message);
                        messageSent = true; // 防止多次清除
                    }
                }
            }
        }

        // 傳送訊息至 WebSocket
        public async Task SendMessageToWebSocket(WebSocket webSocket, string message)
        {
            // _logger.LogInformation($"Sending message to WebSocket: {message}");

            if (webSocket?.State == WebSocketState.Open)
            {
                _logger.LogInformation($"Send message to WebSocket: {message}");
                var buffer = Encoding.UTF8.GetBytes(message);
                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        // WebSocket 連線時，將緩存的訊息發送到客戶端
        public void OnWebSocketConnected()
        {
            _logger.LogInformation("WebSocket connected, sending cached messages.");

            // 使用 ToList() 创建队列的副本，避免在遍历时修改原队列
            // 發送緩存訊息到已連線的 WebSocket
            var messagesToSend = new List<(string message, bool isSent)>(_messageQueue);
            foreach (var (message, isSent) in messagesToSend)
            {
                if (!isSent)
                {
                    BroadcastMessageToWebSockets(message);  // 广播消息到所有连接
                }
            }
            // 清理已发送消息
            _messageQueue.RemoveAll(msg => msg.isSent);
            // CheckStopListening(); // 在 WebSocket 连接后检查是否需要停止监听
        }

        // WebSocket 斷開連線時停止監聽 RabbitMQ
        public void OnWebSocketDisconnected()
        {
            _logger.LogInformation("WebSocket disconnected, checking RabbitMQ listener status.");
            // CheckStopListening(); // 在 WebSocket 断开后检查是否需要停止监听
        }
    }
}
