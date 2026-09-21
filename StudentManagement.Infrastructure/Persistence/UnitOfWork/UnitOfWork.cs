using StudentManagement.Application.Common.Interfaces;
using StudentManagement.Infrastructure.Persistence.Context;

namespace StudentManagement.Infrastructure.Persistence.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StudentManagementDbContext _context;

        public UnitOfWork(StudentManagementDbContext context)
        {
            _context = context;
        }
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
