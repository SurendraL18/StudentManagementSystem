using Microsoft.EntityFrameworkCore;
using Npgsql;
using StudentManagement.Application.Common.Exceptions;
using StudentManagement.Application.Common.Interfaces;
using StudentManagement.Domain.Entities;
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
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {

                if (pgEx.ConstraintName == "IX_users_Email")
                {

                    var userEntry = _context.ChangeTracker.Entries<User>()
                        .FirstOrDefault(e => e.State == EntityState.Added);

                    if (userEntry != null)
                    {
                        string offendingEmail = userEntry.Entity.Email;


                        throw new DuplicateUserEmailException(offendingEmail);
                    }
                }

                throw;
            }
        }
    }
}
