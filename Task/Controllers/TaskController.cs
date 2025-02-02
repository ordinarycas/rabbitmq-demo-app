using Microsoft.AspNetCore.Mvc;
using TaskApp.Services;

namespace TaskApp.Controllers
{
    // 設定 API 路徑前綴為 /api/v1/task
    [Route("api/v1/task")]
    public class TaskController : Controller
    {
        private readonly RabbitMQService _rabbitMQService;

        public TaskController(RabbitMQService rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
        }

        // 設定 API 路徑為 /api/v1/task/sendmessage
        [HttpPost("sendmessage")]
        public IActionResult SendMessage([FromBody] TaskRequest request)
        {
            if (string.IsNullOrEmpty(request.RoutingKey))  // 檢查 routingKey 是否為 null 或空字串
            {
                return BadRequest(new { message = "RoutingKey 不能為空" });
            }

            _rabbitMQService.SendMessage(request.LoopCount, request.RoutingKey);
            return Json(new { message = "訊息已發送" });
        }
    }

    // 請求的資料結構
    public class TaskRequest
    {
        public int LoopCount { get; set; }
        public string? RoutingKey { get; set; }  // 將 RoutingKey 設為 nullable
    }
}
