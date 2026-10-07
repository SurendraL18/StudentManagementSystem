using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Application.Authentication;
using StudentManagement.Application.Authentication.RefreshToken;

namespace StudentManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly LoginService _loginService;
        private readonly RefreshTokenService _refreshTokenService;

        public AuthenticationController(LoginService loginService, RefreshTokenService refreshTokenService)
        {
            _loginService = loginService;
            _refreshTokenService = refreshTokenService;
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

        [HttpPost("refresh")]
        public async Task<ActionResult<RefreshTokenResponse>> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
        {
            var response = await _refreshTokenService.RefreshAsync(
                request,
                cancellationToken);

            return Ok(response);
        }
    }
}
