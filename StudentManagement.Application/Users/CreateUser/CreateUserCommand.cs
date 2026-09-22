using StudentManagement.Domain.Enums;

namespace StudentManagement.Application.Users.CreateUser
{
    public record CreateUserCommand(string Email, string Password, UserRole Role);

}
