using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


// WebSocketService.cs
namespace Worker.Services
{
    public class WebSocketService
    {
        // 存储所有连接的 WebSocket 和它们的连接 ID
        private static List<(string ConnectionId, WebSocket WebSocket)> _connections = new List<(string, WebSocket)>();
        private readonly ILogger<WebSocketService> _logger;
        private readonly object _connectionLock = new object(); // 用于同步 WebSocket 连接数的操作
        private readonly SemaphoreSlim _connectionSemaphore = new SemaphoreSlim(1, 1);

        public WebSocketService(ILogger<WebSocketService> logger)
        {
            _logger = logger;
        }

        // 判断是否有 WebSocket 连接
        public bool IsConnected => _connections.Count > 0;

        // 获取当前 WebSocket 连接的数量
        public int GetCurrentConnectionCount()
        {
            lock (_connectionLock)
            {
                return _connections.Count;
            }
        }

        // 获取所有连接的 Connection ID
        public List<string> GetAllConnectionIds()
        {
            lock (_connectionLock)
            {
                return _connections.Select(c => c.ConnectionId).ToList();
            }
        }

        public List<WebSocket> GetAllWebSockets()
        {
            lock (_connectionLock)
            {
                return _connections.Select(c => c.WebSocket).ToList(); // Get WebSocket objects
            }
        }

        // 当 WebSocket 连接时调用
        public async void OnWebSocketConnected(WebSocket webSocket)
        {
            await _connectionSemaphore.WaitAsync(); // 鎖住
            try
            {
                string connectionId = Guid.NewGuid().ToString(); // 为每个 WebSocket 生成唯一的连接 ID
                _connections.Add((connectionId, webSocket));
                _logger.LogInformation($"WebSocket connected. Current connections: {GetCurrentConnectionCount()}, ID: {connectionId}");
            }
            finally
            {
                _connectionSemaphore.Release(); // 確保釋放
            }
            // 连接成功后广播连接数和所有连接的 ID
            BroadcastCurrentConnections();
        }

        // 当 WebSocket 断开时调用
        public async void OnWebSocketDisconnected(WebSocket webSocket)
        {
            await _connectionSemaphore.WaitAsync();
            try
            {
                var connection = _connections.FirstOrDefault(c => c.WebSocket == webSocket);
                if (connection != default)
                {
                    _connections.Remove(connection);
                    _logger.LogInformation($"WebSocket disconnected. Current connections: {GetCurrentConnectionCount()}, ID: {connection.ConnectionId}");
                }
            }
            finally
            {
                _connectionSemaphore.Release(); // 確保釋放
            }
            // 廣播當前連接
            BroadcastCurrentConnections();
        }

        // 广播当前连接数和所有连接的 ID 到所有 WebSocket 客户端
        private async void BroadcastCurrentConnections()
        {
            var connectionCount = GetCurrentConnectionCount();
            var connectionIds = GetAllConnectionIds();

            // 準備 JSON 資料
            var messageObject = new
            {
                type = "currentConnections",
                connectionCount = connectionCount,
                connectionIds = connectionIds
            };

            string message = JsonConvert.SerializeObject(messageObject);

            await _connectionSemaphore.WaitAsync(); // 獲取鎖
            try
            {
                foreach (var (connectionId, webSocket) in _connections)
                {
                    if (webSocket.State == WebSocketState.Open)
                    {
                        await SendMessageAsync(webSocket, message); // 确保在异步上下文中发送消息
                    }
                }
            }
            finally
            {
                _connectionSemaphore.Release(); // 释放锁
            }
        }

        // 发送消息到 WebSocket
        private async Task SendMessageAsync(WebSocket webSocket, string message)
        {
            if (webSocket?.State != WebSocketState.Open)
                throw new InvalidOperationException("WebSocket is not open.");

            var buffer = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        // 接收来自 WebSocket 的消息
        public async Task ListenWebSocketAsync(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = null;

            while (webSocket.State == WebSocketState.Open)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

                    // WebSocket 断开时移除连接
                    OnWebSocketDisconnected(webSocket);
                }
            }
        }

        // 當 WebSocket 連線時，返回其 ConnectionId
        public async Task<string> OnWebSocketConnectedAsync(WebSocket webSocket)
        {
            await _connectionSemaphore.WaitAsync(); // 使用异步锁
            try
            {
                string connectionId = Guid.NewGuid().ToString(); // 生成唯一連線 ID
                _connections.Add((connectionId, webSocket)); // 儲存 WebSocket 和連線 ID

                _logger.LogInformation($"WebSocket connected. ID: {connectionId}, Current connections: {GetCurrentConnectionCount()}");

                // 傳送給新連線的客戶端：自身連線的 ID
                var initialMessage = new
                {
                    type = "connectionInfo",
                    connectionId = connectionId,
                    connectionCount = GetCurrentConnectionCount(),
                    connectionIds = GetAllConnectionIds()
                };

                await SendMessageAsync(webSocket, JsonConvert.SerializeObject(initialMessage));

                // 廣播當前連線狀態給其他客戶端
                BroadcastCurrentConnections();
                return connectionId;
            }
            finally
            {
                _connectionSemaphore.Release(); // 确保释放锁
            }
        }
    }
}
