using Microsoft.AspNetCore.Mvc;

namespace Matrix.Audio.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class MaintenanceController(ILogger<MaintenanceController> logger) : ControllerBase
{
    private readonly ILogger<MaintenanceController> _logger = logger;

    [HttpGet("echo", Name = "echo")]
    public void Echo()
    {
        _logger.LogInformation("Echo");
    }
}
