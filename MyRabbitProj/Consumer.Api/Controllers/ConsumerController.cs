using Consumer.Api.RabbitMq;
using Microsoft.AspNetCore.Mvc;

namespace Consumer.Api.Controllers;

[ApiController]
[Route("api/consumer")]
public class ConsumerController : ControllerBase
{
    private readonly RabbitMqConsumerService _service;

    public ConsumerController(RabbitMqConsumerService service)
    {
        _service = service;
    }

    [HttpGet("status")]
    public IActionResult Status()
    {
        return Ok(new
        {
            state = _service.State.ToString(),
            isRunning = _service.State == ConsumerState.Running,
            error = _service.LastError,
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = _service.State == ConsumerState.Running ? "OK" : "ERROR",
            service = "RabbitMQ Consumer",
            state = _service.State.ToString()
        });
    }
}