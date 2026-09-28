using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StudentManagement.Application.Common.Exceptions;
using StudentManagement.Domain.Entities;
using StudentManagement.Domain.Enums; // Added to access the UserRole enum
using StudentManagement.Infrastructure.Persistence.Context;
using StudentManagement.Infrastructure.Persistence.UnitOfWork;

namespace StudentManagement.IntegrationTests.Persistence;

public class UnitOfWorkTests : IAsyncLifetime
{
    private StudentManagementDbContext _context = null!;
    private UnitOfWork _unitOfWork = null!;


    public async Task InitializeAsync()
    {
        // 1. Load configuration from configuration files
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // 2. Read DefaultConnection and enforce validation boundaries strictly
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string 'DefaultConnection' is not configured for the Integration Test environment.");
        }

        var options = new DbContextOptionsBuilder<StudentManagementDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        // 3. Instantiate real concrete operational units
        _context = new StudentManagementDbContext(options);
        _unitOfWork = new UnitOfWork(_context);

        // 4. Ensure a completely clean schema lifecycle state layout
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.MigrateAsync();
    }

    [Fact]
    public async Task SaveChangesAsync_WithDuplicatePostgresEmail_ShouldTranslateToDuplicateUserEmailException()
    {
        // Arrange
        const string targetDuplicateEmail = "duplicate@school.com";

        // Fixed: Swapped plain strings for the explicit strongly-typed UserRole enum values
        var initialUser = new User(
            targetDuplicateEmail,
            "InitialEncryptedPasswordHashString",
            UserRole.Student);

        var violatingUser = new User(
            targetDuplicateEmail,
            "SecondaryEncryptedPasswordHashString",
            UserRole.Teacher);

        // 1. Stage and successfully insert the very first profile entry record
        await _context.Users.AddAsync(initialUser);
        await _context.SaveChangesAsync();

        // 2. Clear EF tracked references to mimic a completely separate request thread context
        _context.ChangeTracker.Clear();

        // 3. Queue the second conflicting entity carrying the duplicate identity string property
        await _context.Users.AddAsync(violatingUser);

        // Act & Assert
        // 4. Force save pipeline down to the raw PostgreSQL indexing engine, ensuring code translates 23505 flawlessly
        var exception = await Assert.ThrowsAsync<DuplicateUserEmailException>(async () =>
            await _unitOfWork.SaveChangesAsync(CancellationToken.None));

        // 5. Verify the actual contextual identity value was extracted from the tracker tree correctly
        Assert.Equal(targetDuplicateEmail, exception.Email);
    }

    /// <summary>
    /// Resource teardown cleanup boundary. Resets the integration database state back to baseline.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_context != null)
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }
    }
}
