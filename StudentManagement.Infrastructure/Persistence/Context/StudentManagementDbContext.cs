using Microsoft.EntityFrameworkCore;
using StudentManagement.Domain.Common;
using StudentManagement.Domain.Entities;

namespace StudentManagement.Infrastructure.Persistence.Context
{
    public class StudentManagementDbContext : DbContext
    {
        public StudentManagementDbContext(DbContextOptions<StudentManagementDbContext> options) : base(options)
        {

        }

        public DbSet<Student> Students => Set<Student>();
        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();
        public DbSet<Attendance> Attendances => Set<Attendance>();
        public DbSet<Mark> Marks => Set<Mark>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StudentManagementDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            var utcNow = DateTime.UtcNow;

            foreach (var entry in entries)
            {

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAtUtc = utcNow;
                    entry.Entity.UpdatedAtUtc = utcNow;
                }

                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAtUtc = utcNow;
                    entry.Property(x => x.CreatedAtUtc).IsModified = false;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }


    }
}
