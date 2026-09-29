using StudentManagement.Application.Common.Results;
using StudentManagement.Domain.Enums;

namespace StudentManagement.Application.Common.Interfaces
{
    public interface ITokenService
    {
        TokenResult GenerateTokens(Guid userId, string email, UserRole role);

    }
}
