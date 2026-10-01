using StudentManagement.Domain.Entities;

namespace StudentManagement.Application.Common.Interfaces
{
    public interface IRefreshTokenStore
    {
        Task AddAsync(RefreshToken refreshToken, CancellationToken cancellation = default);
        Task<RefreshToken?> GetByTokenAsync(string tokenHash, CancellationToken cancellationToken = default);
    }
}
