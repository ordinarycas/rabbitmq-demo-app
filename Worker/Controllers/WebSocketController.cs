using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebSockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Worker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Worker.Controllers
{
    [Route("ws")]
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        private readonly ILogger<WebSocketController> _logger;
        private readonly WebSocketService _webSocketService;
        private readonly MessageService _messageService;

        public WebSocketController(WebSocketService webSocketService, MessageService messageService, ILogger<WebSocketController> logger)
        {
            _webSocketService = webSocketService;
            _messageService = messageService;
            _logger = logger;  // 将 logger 注入到 _logger 变量
        }

        [HttpGet]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                
                // 確保連線時設置 WebSocket 並添加到服務中
                // _webSocketService.OnWebSocketConnected(webSocket);
                // 改用 OnWebSocketConnectedAsync 並取得 connectionId
                string connectionId = await _webSocketService.OnWebSocketConnectedAsync(webSocket);

                // 當 WebSocket 連線成功時，若沒有其他 WebSocket 連線，才啟動 RabbitMQ 監聽
                if (_webSocketService.GetCurrentConnectionCount() > 0)
                {
                    _messageService.StartListening();
                    _messageService.OnWebSocketConnected();
                }
                // 處理 WebSocket 的消息傳遞
                await HandleWebSocketConnection(webSocket);

                _logger.LogInformation($"Connection ID: {connectionId}");
                _logger.LogInformation($"GetCurrentConnectionCount: {_webSocketService.GetCurrentConnectionCount()}");
            }
            else
            {
                HttpContext.Response.StatusCode = 400; // 非 WebSocket 請求
            }
        }

        private async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = null;

            try
            {
                while (_webSocketService.IsConnected && webSocket.State == WebSocketState.Open)
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

                        // 當 WebSocket 斷開時，停止 RabbitMQ 監聽
                        _messageService.OnWebSocketDisconnected();
                        _webSocketService.OnWebSocketDisconnected(webSocket);  // 移除 WebSocket 连接
                        break;  // 跳出循環
                    }
                    // 這裡可以處理接收到的資料
                    // 例如解析訊息等
                }
            }
            catch (WebSocketException ex)
            {
                // 這裡處理 WebSocket 相關異常
                Console.WriteLine($"WebSocket error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // 捕獲其他可能的異常
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
            finally
            {
                // 確保在 WebSocket 關閉後進行清理工作
                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseSent)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
                }

                // 確保 WebSocket 斷開後執行清理
                _webSocketService.OnWebSocketDisconnected(webSocket);
            }
        }
    }
}