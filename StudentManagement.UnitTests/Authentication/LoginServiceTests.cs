using StudentManagement.Application.Authentication;
using StudentManagement.Application.Common.Exceptions;
using StudentManagement.Application.Common.Interfaces;
using StudentManagement.Application.Common.Results;
using StudentManagement.Domain.Common;
using StudentManagement.Domain.Entities;
using StudentManagement.Domain.Enums;

namespace StudentManagement.UnitTests.Authentication;

public class LoginServiceTests
{
    private readonly FakeUserStore _userStore;
    private readonly FakePasswordHasher _passwordHasher;
    private readonly FakeTokenService _tokenService;
    private readonly FakeRefreshTokenStore _refreshTokenStore;
    private readonly FakeRefreshTokenHasher _refreshTokenHasher;
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly FakeAuthenticationSettings _authenticationSettings;

    private readonly LoginService _sut;

    public LoginServiceTests()
    {
        _userStore = new FakeUserStore();
        _passwordHasher = new FakePasswordHasher();
        _tokenService = new FakeTokenService();
        _refreshTokenStore = new FakeRefreshTokenStore();
        _refreshTokenHasher = new FakeRefreshTokenHasher();
        _unitOfWork = new FakeUnitOfWork();
        _authenticationSettings = new FakeAuthenticationSettings();

        _sut = new LoginService(
            _tokenService,
            _userStore,
            _passwordHasher,
            _refreshTokenStore,
            _unitOfWork,
            _refreshTokenHasher,
            _authenticationSettings);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnLoginResponse_WhenUserIsActive()
    {
        // Arrange
        var user = new User(
            "active@school.com",
            "HashedSecurePassword",
            UserRole.Student);

        SetEntityId(user);

        _userStore.Users.Add(user);

        var request = new LoginRequest(
            "active@school.com",
            "PlaintextPassword");

        var expectedHash = "hashed-fake-refresh-token";

        var testStart = DateTime.UtcNow;

        // Act
        var response = await _sut.LoginAsync(
            request,
            CancellationToken.None);

        // Assert
        Assert.NotNull(response);

        Assert.Equal(
            "fake-access-token",
            response.AccessToken);

        Assert.Equal(
            "fake-refresh-token",
            response.RefreshToken);

        Assert.Equal(
            _tokenService.ExpectedExpiration,
            response.AccessTokenExpiresAt);

        Assert.True(
            _tokenService.WasGenerateTokensCalled,
            "Token generation should have been invoked for a valid user.");

        // Refresh token must be hashed before persistence.
        Assert.True(
            _refreshTokenHasher.WasHashCalled,
            "Refresh token must be hashed before persistence.");

        // Refresh token must be persisted.
        Assert.Single(
            _refreshTokenStore.RefreshTokens);

        var savedToken =
            _refreshTokenStore.RefreshTokens.Single();

        // The database representation must contain the hash.
        Assert.Equal(
            expectedHash,
            savedToken.Token);

        // The raw refresh token must never be persisted.
        Assert.NotEqual(
            "fake-refresh-token",
            savedToken.Token);

        // Refresh token must belong to authenticated user.
        Assert.Equal(
            user.Id,
            savedToken.UserId);

        // Refresh token lifetime must use configured value.
        var expectedExpiration =
            testStart.AddDays(
                _authenticationSettings.RefreshTokenExpirationDays);

        Assert.InRange(
            savedToken.ExpiresAtUtc,
            expectedExpiration.AddSeconds(-2),
            expectedExpiration.AddSeconds(2));

        // Persistence must be committed.
        Assert.True(
            _unitOfWork.WasSaveChangesCalled,
            "Refresh token persistence must be committed.");
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest(
            "nonexistent@school.com",
            "AnyPassword");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            async () =>
                await _sut.LoginAsync(
                    request,
                    CancellationToken.None));

        Assert.False(
            _tokenService.WasGenerateTokensCalled,
            "Token generation must not be invoked for a non-existent email.");

        Assert.False(
            _refreshTokenHasher.WasHashCalled,
            "Refresh token hashing must not occur when authentication fails.");

        Assert.Empty(
            _refreshTokenStore.RefreshTokens);

        Assert.False(
            _unitOfWork.WasSaveChangesCalled,
            "No persistence should occur when authentication fails.");
        Assert.False(
         _passwordHasher.WasVerifyCalled,
         "Password verification must not occur when the user does not exist.");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldNotInvokeTokenGenerationService()
    {
        // Arrange
        var user = new User(
            "active@school.com",
            "HashedSecurePassword",
            UserRole.Student);

        SetEntityId(user);

        _userStore.Users.Add(user);

        var request = new LoginRequest(
            "active@school.com",
            "IncorrectPlaintextPassword");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            async () =>
                await _sut.LoginAsync(
                    request,
                    CancellationToken.None));

        Assert.False(
            _tokenService.WasGenerateTokensCalled,
            "ITokenService.GenerateTokens must never be called when password validation fails.");

        Assert.False(
            _refreshTokenHasher.WasHashCalled,
            "Refresh token hashing must not occur when password validation fails.");

        Assert.Empty(
            _refreshTokenStore.RefreshTokens);

        Assert.False(
            _unitOfWork.WasSaveChangesCalled,
            "No persistence should occur when authentication fails.");
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUserStatus_ShouldThrowInvalidCredentialsException_EvenIfPasswordIsCorrect()
    {
        // Arrange
        var user = new User(
            "inactive@school.com",
            "HashedSecurePassword",
            UserRole.Student);

        SetEntityId(user);

        user.Deactivate();

        _userStore.Users.Add(user);

        var request = new LoginRequest(
            "inactive@school.com",
            "PlaintextPassword");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            async () =>
                await _sut.LoginAsync(
                    request,
                    CancellationToken.None));

        Assert.False(
            _tokenService.WasGenerateTokensCalled,
            "Token generation must not be invoked for an inactive user.");

        Assert.False(
            _refreshTokenHasher.WasHashCalled,
            "Refresh token hashing must not occur for an inactive user.");

        Assert.Empty(
            _refreshTokenStore.RefreshTokens);

        Assert.False(
            _unitOfWork.WasSaveChangesCalled,
            "No persistence should occur for an inactive user.");
    }

    [Fact]
    public async Task LoginAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () =>
                await _sut.LoginAsync(
                    null!,
                    CancellationToken.None));

        Assert.False(
            _tokenService.WasGenerateTokensCalled,
            "Token generation must not occur when the login request is null.");

        Assert.False(
            _refreshTokenHasher.WasHashCalled,
            "Refresh token hashing must not occur when the login request is null.");

        Assert.Empty(
            _refreshTokenStore.RefreshTokens);

        Assert.False(
            _unitOfWork.WasSaveChangesCalled,
            "No persistence should occur when the login request is null.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task LoginAsync_WithEmptyOrWhitespacePassword_ShouldThrowInvalidCredentialsException(
        string password)
    {
        // Arrange
        var request = new LoginRequest(
            "active@school.com",
            password);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            async () =>
                await _sut.LoginAsync(
                    request,
                    CancellationToken.None));

        Assert.False(
            _tokenService.WasGenerateTokensCalled,
            "Token generation must not occur when the password is empty.");

        Assert.False(
            _refreshTokenHasher.WasHashCalled,
            "Refresh token hashing must not occur when the password is empty.");

        Assert.Empty(
            _refreshTokenStore.RefreshTokens);

        Assert.False(
            _unitOfWork.WasSaveChangesCalled,
            "No persistence should occur when the password is empty.");
    }

    [Fact]
    public async Task LoginAsync_WithUppercaseAndWhitespaceEmail_ShouldAuthenticateSuccessfully()
    {
        // Arrange
        var user = new User(
            "active@school.com",
            "HashedSecurePassword",
            UserRole.Student);

        SetEntityId(user);

        _userStore.Users.Add(user);

        var request = new LoginRequest(
            "  ACTIVE@SCHOOL.COM  ",
            "PlaintextPassword");

        // Act
        var response = await _sut.LoginAsync(
            request,
            CancellationToken.None);

        // Assert
        Assert.NotNull(response);

        Assert.Equal(
            "fake-access-token",
            response.AccessToken);

        Assert.Equal(
            "fake-refresh-token",
            response.RefreshToken);

        Assert.True(
            _tokenService.WasGenerateTokensCalled,
            "Token generation should occur after successful email normalization.");

        Assert.True(
            _refreshTokenHasher.WasHashCalled,
            "Refresh token must be hashed after successful authentication.");

        Assert.Single(
            _refreshTokenStore.RefreshTokens);

        Assert.True(
            _unitOfWork.WasSaveChangesCalled,
            "Successful authentication should persist the refresh token.");
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUserStatus_ShouldNotHashOrPersistRefreshToken()
    {
        // Arrange
        var user = new User(
            "inactive@school.com",
            "HashedSecurePassword",
            UserRole.Student);

        SetEntityId(user);

        user.Deactivate();

        _userStore.Users.Add(user);

        var request = new LoginRequest(
            "inactive@school.com",
            "PlaintextPassword");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            async () =>
                await _sut.LoginAsync(
                    request,
                    CancellationToken.None));

        Assert.False(
            _tokenService.WasGenerateTokensCalled,
            "Token generation must not occur for an inactive user.");

        Assert.False(
            _refreshTokenHasher.WasHashCalled,
            "Refresh token hashing must not occur for an inactive user.");

        Assert.Empty(
            _refreshTokenStore.RefreshTokens);

        Assert.False(
            _unitOfWork.WasSaveChangesCalled,
            "No persistence should occur for an inactive user.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task LoginAsync_WithEmptyOrWhitespaceEmail_ShouldThrowInvalidCredentialsException(
    string email)
    {
        // Arrange
        var request = new LoginRequest(
            email,
            "PlaintextPassword");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            async () =>
                await _sut.LoginAsync(
                    request,
                    CancellationToken.None));

        Assert.False(
            _tokenService.WasGenerateTokensCalled,
            "Token generation must not occur when the email is empty.");

        Assert.False(
            _refreshTokenHasher.WasHashCalled,
            "Refresh token hashing must not occur when the email is empty.");

        Assert.Empty(
            _refreshTokenStore.RefreshTokens);

        Assert.False(
            _unitOfWork.WasSaveChangesCalled,
            "No persistence should occur when the email is empty.");
    }

    #region Test Helpers

    private static void SetEntityId(BaseEntity entity)
    {
        typeof(BaseEntity)
            .GetProperty(nameof(BaseEntity.Id))!
            .SetValue(entity, Guid.NewGuid());
    }

    #endregion

    #region Hand-Written Test Doubles

    private class FakeUserStore : IUserStore
    {
        public List<User> Users { get; } = [];

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            var normalizedEmail =
                email.Trim().ToLowerInvariant();

            var user = Users.SingleOrDefault(
                u => u.Email == normalizedEmail);

            return Task.FromResult(user);
        }

        public Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            Users.Add(user);

            return Task.CompletedTask;
        }

        Task<User?> IUserStore.GetByIdAsync(Guid Id, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }
    }

    private class FakePasswordHasher : IPasswordHasher
    {
        public bool WasVerifyCalled { get; private set; }

        public string Hash(string password)
        {
            return $"Hashed{password}";
        }

        public bool Verify(
            string password,
            string passwordHash)
        {
            WasVerifyCalled = true;

            return passwordHash == "HashedSecurePassword"
                   && password == "PlaintextPassword";
        }
    }

    private class FakeTokenService : ITokenService
    {
        public DateTimeOffset ExpectedExpiration { get; } =
            DateTimeOffset.UtcNow.AddHours(1);

        public bool WasGenerateTokensCalled { get; private set; }

        public TokenResult GenerateTokens(
            Guid userId,
            string email,
            UserRole role)
        {
            WasGenerateTokensCalled = true;

            return new TokenResult(
                "fake-access-token",
                "fake-refresh-token",
                ExpectedExpiration);
        }
    }

    private class FakeRefreshTokenStore : IRefreshTokenStore
    {
        public List<RefreshToken> RefreshTokens { get; } = [];

        public Task AddAsync(
            RefreshToken refreshToken,
            CancellationToken cancellationToken = default)
        {
            RefreshTokens.Add(refreshToken);

            return Task.CompletedTask;
        }

        public Task<RefreshToken?> GetByTokenAsync(
            string tokenHash,
            CancellationToken cancellationToken = default)
        {
            var refreshToken =
                RefreshTokens.SingleOrDefault(
                    x => x.Token == tokenHash);

            return Task.FromResult(refreshToken);
        }
    }

    private class FakeRefreshTokenHasher : IRefreshTokenHasher
    {
        public bool WasHashCalled { get; private set; }

        public string Hash(string refreshToken)
        {
            WasHashCalled = true;

            return $"hashed-{refreshToken}";
        }
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public bool WasSaveChangesCalled { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            WasSaveChangesCalled = true;

            return Task.FromResult(1);
        }
    }

    private class FakeAuthenticationSettings : IAuthenticationSettings
    {
        public int RefreshTokenExpirationDays { get; } = 7;
    }

    #endregion
}
