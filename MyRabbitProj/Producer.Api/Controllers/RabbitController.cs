using Microsoft.AspNetCore.Mvc;
using RabbitMqProducer.RabbitMq;

namespace Producer.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RabbitController : ControllerBase
    {
        private readonly IRabbitMqService _rabbitMqService;

        public RabbitController(IRabbitMqService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] string message)
        {
            await _rabbitMqService.SendMessageAsync(message);
            return Ok(new { status = "Сообщение отправлено", message });
        }
    }
}

