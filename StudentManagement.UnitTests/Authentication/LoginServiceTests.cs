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

        // Match exact constructor order: ITokenService -> IUserStore -> IPasswordHasher
        _sut = new LoginService(_tokenService, _userStore, _passwordHasher);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnLoginResponse_WhenUserIsActive()
    {
        // Arrange
        // Assuming default constructor sets status to Active, or adjust constructor signature if needed
        var user = new User("active@school.com", "HashedSecurePassword", UserRole.Student);

        // If Status cannot be set externally, ensure your default User domain entity constructor initializes it as Active.
        _userStore.Users.Add(user);

        var request = new LoginRequest("active@school.com", "PlaintextPassword");

        // Act
        var response = await _sut.LoginAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("fake-access-token", response.AccessToken);
        Assert.Equal("fake-refresh-token", response.RefreshToken);
        Assert.Equal(_tokenService.ExpectedExpiration, response.AccessTokenExpiresAt);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest("nonexistent@school.com", "AnyPassword");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(async () =>
            await _sut.LoginAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var user = new User("active@school.com", "HashedSecurePassword", UserRole.Student);
        _userStore.Users.Add(user);

        var request = new LoginRequest("active@school.com", "IncorrectPlaintextPassword");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(async () =>
            await _sut.LoginAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUserStatus_ShouldThrowInvalidCredentialsException_EvenIfPasswordIsCorrect()
    {
        // Arrange
        var user = new User("inactive@school.com", "HashedSecurePassword", UserRole.Student);

        // Use your domain's encapsulation method to transition status if setter is private (e.g., user.Deactivate())
        // If an explicit method doesn't exist, we mimic the database load state by adding an inactive user to the fake store
        _userStore.SimulateInactiveUserLoad(user);

        var request = new LoginRequest("inactive@school.com", "PlaintextPassword");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(async () =>
            await _sut.LoginAsync(request, CancellationToken.None));
    }

    #region Hand-Written Test Doubles

    private class FakeUserStore : IUserStore
    {
        public List<User> Users { get; } = [];
        private readonly List<string> _inactiveEmails = [];

        public void SimulateInactiveUserLoad(User user)
        {
            Users.Add(user);
            _inactiveEmails.Add(user.Email);
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        {
            var user = Users.SingleOrDefault(u => u.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase));

            // If we simulated this user as inactive, intercept and override the read-only property value using reflection if necessary,
            // or use a clean fake runtime state strategy:
            if (user != null && _inactiveEmails.Contains(user.Email))
            {
                var statusField = typeof(User).GetProperty("Status");
                if (statusField != null && statusField.CanWrite)
                {
                    statusField.SetValue(user, UserStatus.Inactive);
                }
                else
                {
                    // Fallback to backing field if no setter exists at all
                    var backingField = typeof(User).GetField("<Status>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    backingField?.SetValue(user, UserStatus.Inactive);
                }
            }

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

        public TokenResult GenerateTokens(Guid userId, string email, UserRole role)
        {
            return new TokenResult(
                "fake-access-token",
                "fake-refresh-token",
                ExpectedExpiration);
        }
    }

    #endregion
}
