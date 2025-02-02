using Microsoft.AspNetCore.Mvc;
using TaskApp.Services;

namespace TaskApp.Controllers
{
    // �]�w API ���|�e�� /api/v1/task
    [Route("api/v1/task")]
    public class TaskController : Controller
    {
        private readonly RabbitMQService _rabbitMQService;

        public TaskController(RabbitMQService rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
        }

        // �]�w API ���|�� /api/v1/task/sendmessage
        [HttpPost("sendmessage")]
        public IActionResult SendMessage([FromBody] TaskRequest request)
        {
            if (string.IsNullOrEmpty(request.RoutingKey))  // �ˬd routingKey �O�_�� null �ΪŦr��
            {
                return BadRequest(new { message = "RoutingKey ���ର��" });
            }

            _rabbitMQService.SendMessage(request.LoopCount, request.RoutingKey);
            return Json(new { message = "�T���w�o�e" });
        }
    }

    // �ШD����Ƶ��c
    public class TaskRequest
    {
        public int LoopCount { get; set; }
        public string? RoutingKey { get; set; }  // �N RoutingKey �]�� nullable
    }
}
