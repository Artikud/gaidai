using Microsoft.AspNetCore.Mvc;

namespace ATI.Gaidai.Controllers
{
    public class HealthCheckController : Controller
    {
        [Route("_internal/healthcheck")]
        public IActionResult HealthCheckAsync()
        {
            return Ok();
        }
    }
}
