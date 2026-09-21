using Microsoft.EntityFrameworkCore;
using StudentManagement.Application.Common.Interfaces;
using StudentManagement.Domain.Entities;
using StudentManagement.Infrastructure.Persistence.Context;

namespace StudentManagement.Infrastructure.Persistence.Stores
{
    public class UserStore : IUserStore
    {
        private readonly StudentManagementDbContext _context;

        public UserStore(StudentManagementDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "Cannot add a null user entity to the persistence tracking pool.");
            }

            await _context.Users.AddAsync(user, cancellationToken);
        }



        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }


            var normalizedEmail = email.Trim().ToLowerInvariant();

            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        }
    }
}
