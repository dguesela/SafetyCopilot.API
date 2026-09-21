using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafetyCopilot.API.Data;

namespace SafetyCopilot.API.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        private readonly SafetyDbContext _db;

        public HealthController(
            SafetyDbContext db)
        {
            _db = db;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var database =
                await _db.Database.CanConnectAsync();

            return Ok(
                new
                {
                    status =
                        database
                            ? "Healthy"
                            : "Degraded",

                    api = "Healthy",

                    database,

                    utc = DateTime.UtcNow
                });
        }
    }
}