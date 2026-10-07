using Microsoft.EntityFrameworkCore;
using StudentManagement.Application.Common.Interfaces;
using StudentManagement.Domain.Entities;
using StudentManagement.Infrastructure.Persistence.Context;

namespace StudentManagement.Infrastructure.Persistence.Stores
{
    public class RefreshTokenStore : IRefreshTokenStore
    {
        private readonly StudentManagementDbContext _context;

        public RefreshTokenStore(StudentManagementDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellation = default)
        {
            if (refreshToken is null)
            {
                throw new ArgumentNullException(nameof(refreshToken), "Cannot persist a null refresh token entity.");
            }

            // Add the entity to the tracked context collection without calling SaveChangesAsync
            await _context.Set<RefreshToken>().AddAsync(refreshToken, cancellation);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tokenHash))
            {
                return null;
            }


            return await _context.Set<RefreshToken>()
                .SingleOrDefaultAsync(rt => rt.Token == tokenHash, cancellationToken);
        }
    }
}
