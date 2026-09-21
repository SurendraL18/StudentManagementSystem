using StudentManagement.Domain.Enums;

namespace StudentManagement.Application.Users.CreateUser
{
    public record CreateUserResponse(Guid Id, string Email, UserRole Role, DateTime CreatedAtUtc);

}
