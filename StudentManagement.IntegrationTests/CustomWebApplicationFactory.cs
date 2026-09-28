using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Infrastructure.Persistence.Context;

namespace StudentManagement.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<global::Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Server=dummy;Database=dummy;" }
            });
        });

        builder.ConfigureServices(services =>
        {
            // 1. Locate and remove the existing PostgreSQL DbContext configuration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<StudentManagementDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // 2. Inject an isolated In-Memory Database for this test run context
            services.AddDbContext<StudentManagementDbContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTestsDb");
            });

            // 3. Build a temporary service provider to ensure the database schema exists
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StudentManagementDbContext>();

            db.Database.EnsureCreated();
        });
    }
}
