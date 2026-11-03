using Microsoft.AspNetCore.Mvc;

namespace SalesFlow.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { Status = "Healthy", Service = "SalesFlow API", TimestampUtc = DateTimeOffset.UtcNow });
}
