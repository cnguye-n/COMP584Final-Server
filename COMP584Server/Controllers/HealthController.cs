using Microsoft.AspNetCore.Mvc;

namespace COMP584Server.Controllers
{
    [ApiController]
    public class HealthController : ControllerBase
    {
        // GET /health
        [HttpGet("/health")]
        public IActionResult Health() => Ok("ok");
    }
}
