using Microsoft.AspNetCore.Mvc;
using StudentManagement.Infrastructure.Persistence.Context;

namespace StudentManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly StudentManagementDbContext _context;

        public HealthController(StudentManagementDbContext context)
        {
            _context = context;
        }

        [HttpGet("database")]
        public async Task<IActionResult> CheckDatabaseConnection()
        {
            try
            {
                // Proves the application can connect to PostgreSQL and execute a quick command
                bool canConnect = await _context.Database.CanConnectAsync();

                if (canConnect)
                {
                    return Ok(new { status = "Healthy", message = "Successfully connected to PostgreSQL." });
                }

                return StatusCode(500, new { status = "Unhealthy", message = "Could not establish a connection to the database." });
            }
            catch (Exception ex)
            {
                // Catches connection timeouts, bad credentials, or unreachable host errors
                return StatusCode(500, new { status = "Error", message = ex.Message });
            }
        }
    }
}
