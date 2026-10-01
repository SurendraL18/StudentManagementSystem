using StudentManagement.Application.Authentication;
using StudentManagement.Application.Common.Exceptions;
using StudentManagement.Application.Common.Interfaces;
using StudentManagement.Application.Common.Results;
using StudentManagement.Domain.Entities;
using StudentManagement.Domain.Enums;

namespace StudentManagement.UnitTests.Authentication;

public class LoginServiceTests
{
    private readonly FakeUserStore _userStore;
    private readonly FakePasswordHasher _passwordHasher;
    private readonly FakeTokenService _tokenService;
    private readonly LoginService _sut;

    public LoginServiceTests()
    {
        _userStore = new FakeUserStore();
        _passwordHasher = new FakePasswordHasher();
        _tokenService = new FakeTokenService();

        _sut = new LoginService(_tokenService, _userStore, _passwordHasher);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnLoginResponse_WhenUserIsActive()
    {
        // Arrange
        var user = new User("active@school.com", "HashedSecurePassword", UserRole.Student);
        _userStore.Users.Add(user);

        var request = new LoginRequest("active@school.com", "PlaintextPassword");

        // Act
        var response = await _sut.LoginAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("fake-access-token", response.AccessToken);
        Assert.Equal("fake-refresh-token", response.RefreshToken);
        Assert.Equal(_tokenService.ExpectedExpiration, response.AccessTokenExpiresAt);
        Assert.True(_tokenService.WasGenerateTokensCalled, "Token generation should have been invoked for a valid user.");
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest("nonexistent@school.com", "AnyPassword");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(async () =>
            await _sut.LoginAsync(request, CancellationToken.None));

        Assert.False(_tokenService.WasGenerateTokensCalled, "Token generation must not be invoked for a non-existent email.");
    }

    // [Fact]
    //public async Task LoginAsync_WithWrongPassword_ShouldThrowInvalidCredentialsException()
    //{
    //    // Arrange
    //    var user = new User("active@school.com", "HashedSecurePassword", UserRole.Student);
    //    _userStore.Users.Add(user);

    //    var request = new LoginRequest("active@school.com", "IncorrectPlaintextPassword");

    //    // Act & Assert
    //    await Assert.ThrowsAsync<InvalidCredentialsException>(async () =>
    //        await _sut.LoginAsync(request, CancellationToken.None));
    //}

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldNotInvokeTokenGenerationService()
    {
        // Arrange - Setup a valid user but supply an incorrect password string
        var user = new User("active@school.com", "HashedSecurePassword", UserRole.Student);
        _userStore.Users.Add(user);

        var request = new LoginRequest("active@school.com", "IncorrectPlaintextPassword");

        // Act - Trigger compilation flow block which breaks at password gate evaluation
        await Assert.ThrowsAsync<InvalidCredentialsException>(async () =>
            await _sut.LoginAsync(request, CancellationToken.None));

        // Assert - Explicitly verify that our state tracker shows zero interaction
        Assert.False(_tokenService.WasGenerateTokensCalled, "ITokenService.GenerateTokens must never be called when the password validation check fails.");
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUserStatus_ShouldThrowInvalidCredentialsException_EvenIfPasswordIsCorrect()
    {
        // Arrange
        var user = new User("inactive@school.com", "HashedSecurePassword", UserRole.Student);
        user.Deactivate();
        _userStore.Users.Add(user);

        var request = new LoginRequest("inactive@school.com", "PlaintextPassword");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(async () =>
            await _sut.LoginAsync(request, CancellationToken.None));

        Assert.False(_tokenService.WasGenerateTokensCalled, "Token generation must not be invoked for an inactive user profile.");
    }

    #region Hand-Written Test Doubles

    private class FakeUserStore : IUserStore
    {
        public List<User> Users { get; } = [];

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        {
            var user = Users.SingleOrDefault(u => u.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase));
            return Task.FromResult(user);
        }

        public Task AddAsync(User user, CancellationToken cancellationToken)
        {
            Users.Add(user);
            return Task.CompletedTask;
        }
    }

    private class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"Hashed{password}";

        public bool Verify(string password, string passwordHash)
        {
            return passwordHash == "HashedSecurePassword" && password == "PlaintextPassword";
        }
    }

    private class FakeTokenService : ITokenService
    {
        public DateTimeOffset ExpectedExpiration { get; } = DateTimeOffset.UtcNow.AddHours(1);

        // Added behavioral state tracker property flag
        public bool WasGenerateTokensCalled { get; private set; }

        public TokenResult GenerateTokens(Guid userId, string email, UserRole role)
        {
            WasGenerateTokensCalled = true; // Record the invocation event

            return new TokenResult(
                "fake-access-token",
                "fake-refresh-token",
                ExpectedExpiration);
        }
    }

    #endregion
}
