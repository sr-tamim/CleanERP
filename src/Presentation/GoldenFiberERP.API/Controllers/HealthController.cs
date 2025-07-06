using Microsoft.AspNetCore.Mvc;

namespace GoldenFiberERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Application = "GoldenFiberERP API",
            Version = "1.0.0"
        });
    }
}
