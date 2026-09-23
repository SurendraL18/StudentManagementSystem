using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudentManagement.Application.Common.Interfaces;
using StudentManagement.Infrastructure.Persistence.Context;
using StudentManagement.Infrastructure.Persistence.Stores;
using StudentManagement.Infrastructure.Persistence.UnitOfWork;
using StudentManagement.Infrastructure.Security;

namespace StudentManagement.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "Database connection string 'DefaultConnection' is not configured.");

            services.AddDbContext<StudentManagementDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(StudentManagementDbContext).Assembly.FullName);
            }));

            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IUserStore, UserStore>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
