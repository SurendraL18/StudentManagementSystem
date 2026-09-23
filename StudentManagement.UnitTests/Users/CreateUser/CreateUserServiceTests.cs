using FluentValidation;
using StudentManagement.Application.Common.Interfaces;
using StudentManagement.Application.Users.CreateUser;
using StudentManagement.Domain.Entities;
using StudentManagement.Domain.Enums;

namespace StudentManagement.UnitTests.Users.CreateUser;

public class CreateUserServiceTests
{
    [Fact]
    public async Task CreateAsync_WithInvalidEmail_ShouldThrowValidationExceptionAndFailFast()
    {
        // 1. Arrange (Set up our isolated test environment)
        var fakeUserStore = new FakeUserStore();
        var fakePasswordHasher = new FakePasswordHasher();
        var fakeUnitOfWork = new FakeUnitOfWork();

        // As per requirements, we use the REAL validator to test actual rules
        var realValidator = new CreateUserCommandValidator();

        var service = new CreateUserService(
            fakePasswordHasher,
            fakeUserStore,
            fakeUnitOfWork,
            realValidator);

        // Intentionally malformed payload (Invalid Email format)
        var invalidCommand = new CreateUserCommand(
            Email: "not-a-valid-email",
            Password: "SecurePassword123!",
            Role: UserRole.Student);

        // 2. Act & Assert (Verify ValidationException is thrown)
        await Assert.ThrowsAsync<ValidationException>(async () =>
            await service.CreateAsync(invalidCommand, CancellationToken.None));

        // 3. Assert Boundaries (Verify infrastructure was NEVER invoked)
        Assert.False(fakeUserStore.WasGetByEmailCalled, "Security Violation: IUserStore.GetByEmailAsync should not be called if validation fails.");
        Assert.False(fakeUserStore.WasAddCalled, "Security Violation: IUserStore.AddAsync should not be called if validation fails.");
        Assert.False(fakeUnitOfWork.WasSaveChangesAsyncCalled, "Security Violation: IUnitOfWork.SaveChangesAsync should not be called if validation fails.");
    }

    [Fact]
    public async Task CreateAsync_WithValidCommand_ShouldCreateUserAndSaveChanges()
    {
        // 1. Arrange
        var fakeUserStore = new FakeUserStore();
        var fakePasswordHasher = new FakePasswordHasher();
        var fakeUnitOfWork = new FakeUnitOfWork();
        var realValidator = new CreateUserCommandValidator();

        var service = new CreateUserService(
            fakePasswordHasher,
            fakeUserStore,
            fakeUnitOfWork,
            realValidator);

        var validCommand = new CreateUserCommand(
            Email: "testuser@management.com",
            Password: "StrongPassword999!",
            Role: UserRole.Teacher);

        // 2. Act
        var response = await service.CreateAsync(validCommand, CancellationToken.None);

        // 3. Assert Infrastructure State Mutations
        Assert.True(fakeUserStore.WasGetByEmailCalled, "Execution Failure: IUserStore.GetByEmailAsync must be invoked to verify uniqueness.");
        Assert.True(fakeUserStore.WasAddCalled, "Execution Failure: IUserStore.AddAsync must be invoked to stage the created entity.");
        Assert.True(fakeUnitOfWork.WasSaveChangesAsyncCalled, "Execution Failure: IUnitOfWork.SaveChangesAsync must be invoked to commit transaction boundaries.");

        // 4. Assert Output Integrity & Data Protection Boundaries
        Assert.NotNull(response);
        Assert.Equal("testuser@management.com", response.Email);
        Assert.Equal(UserRole.Teacher, response.Role);

        // 5. Assert Security Boundary: Plaintext or hash states must never leak into Response DTOs
        var stringifiedResponse = response.ToString();
        Assert.DoesNotContain("StrongPassword999!", stringifiedResponse);
        Assert.DoesNotContain("FakeSecuredHashString", stringifiedResponse);

        // 6. Verify Cryptographic Integrity on the underlying tracked data
        Assert.NotNull(fakeUserStore.SavedUser);
        Assert.Equal("FakeSecuredHashString", fakeUserStore.SavedUser.PasswordHash);
    }
}

#region Hand-Rolled Test Doubles (Fakes)

public class FakeUserStore : IUserStore
{
    public bool WasGetByEmailCalled { get; private set; } = false;
    public bool WasAddCalled { get; private set; } = false;

    // Minimal fake-state tracking property to record the intercepted user entity
    public User? SavedUser { get; private set; }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        WasGetByEmailCalled = true;
        return Task.FromResult<User?>(null);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        WasAddCalled = true;
        SavedUser = user; // Catches the in-memory object entity state configuration passed from the orchestrator
        return Task.CompletedTask;
    }
}

public class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return "FakeSecuredHashString";
    }

    public bool Verify(string password, string hashedPassword)
    {
        return true;
    }
}

public class FakeUnitOfWork : IUnitOfWork
{
    public bool WasSaveChangesAsyncCalled { get; private set; } = false;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        WasSaveChangesAsyncCalled = true;
        return Task.FromResult(1);
    }
}

#endregion
