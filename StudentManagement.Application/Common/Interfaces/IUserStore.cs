using StudentManagement.Domain.Entities;

namespace StudentManagement.Application.Common.Interfaces
{
    public interface IUserStore
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        Task<User?> GetByIdAsync(Guid Id, CancellationToken cancellation = default);

        Task AddAsync(User user, CancellationToken cancellationToken = default);

    }
}
