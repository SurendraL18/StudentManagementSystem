using Microsoft.AspNetCore.Mvc;
using StudentManagement.Application.Users.CreateUser;

namespace StudentManagement.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly CreateUserService _createUserService;

        public UsersController(CreateUserService createUserService)
        {
            _createUserService = createUserService;
        }
        [HttpPost]
        public async Task<ActionResult<CreateUserResponse>> CreateUser([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
        {
            var response = await _createUserService.CreateAsync(command, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, response);
        }


    }
}
