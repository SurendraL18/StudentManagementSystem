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
    }
}
