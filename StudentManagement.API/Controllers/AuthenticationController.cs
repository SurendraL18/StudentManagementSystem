using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Application.Authentication;

namespace StudentManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly LoginService _loginService;

        public AuthenticationController(LoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var response = await _loginService.LoginAsync(request, cancellationToken);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("me")]
        public ActionResult GetCurrentClaimsIdentity()
        {
            // Extract required values directly from the authenticated HttpContext.User instance
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");

            var email = User.FindFirstValue(ClaimTypes.Email)
                        ?? User.FindFirstValue("email");

            var role = User.FindFirstValue(ClaimTypes.Role)
                       ?? User.FindFirstValue("role");

            // Return a flattened anonymous payload representing the client-side profile requirements
            return Ok(new
            {
                UserId = userId,
                Email = email,
                Role = role
            });
        }
    }
}
