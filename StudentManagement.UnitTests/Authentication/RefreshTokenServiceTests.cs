using StudentManagement.Application.Authentication;
using StudentManagement.Application.Authentication.RefreshToken;
using StudentManagement.Application.Common.Exceptions;
using StudentManagement.Application.Common.Interfaces;
using StudentManagement.Application.Common.Results;
using StudentManagement.Domain.Common;
using StudentManagement.Domain.Entities;
using StudentManagement.Domain.Enums;

public class RefreshTokenServiceTests
{
    private readonly FakeRefreshTokenHasher _refreshTokenHasher;
    private readonly FakeRefreshTokenStore _refreshTokenStore;
    private readonly FakeUserStore _userStore;
    private readonly FakeTokenService _tokenService;
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly FakeAuthenticationSettings _authenticationSettings;
    private readonly RefreshTokenService _sut;

    public RefreshTokenServiceTests()
    {
        _refreshTokenHasher = new FakeRefreshTokenHasher();
        _refreshTokenStore = new FakeRefreshTokenStore();
        _userStore = new FakeUserStore();
        _tokenService = new FakeTokenService();
        _unitOfWork = new FakeUnitOfWork();
        _authenticationSettings = new FakeAuthenticationSettings();

        _sut = new RefreshTokenService(
            _refreshTokenHasher,
            _refreshTokenStore,
            _userStore,
            _tokenService,
            _unitOfWork,
            _authenticationSettings);
    }

    [Fact]

    public async Task RefreshAsync_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        var user = new User(
           "active@school.com",
           "HashedPassword",
           UserRole.Student);

        // FIX: Safely force the Guid into the entity, regardless of Domain-Driven base classes
        ForceSetId(user, Guid.NewGuid());

        _userStore.Users.Add(user);

        var rawOldToken = "valid-old-refresh-token";
        var expectedOldTokenHash = $"hashed-{rawOldToken}";

        // Now user.Id is guaranteed to be a valid Guid
        var oldRefreshToken = CreateRefreshToken(user.Id, expectedOldTokenHash, TimeSpan.FromDays(7));

        _refreshTokenStore.RefreshTokens.Add(oldRefreshToken);

        var request = new RefreshTokenRequest(rawOldToken);

        // Act
        var response = await _sut.RefreshAsync(
            request,
            CancellationToken.None);

        // Assert API Response
        Assert.NotNull(response);
        Assert.Equal("new-access-token", response.AccessToken);
        Assert.Equal("new-refresh-token", response.RefreshToken);

        // Assert Store State: We should now have exactly 2 tokens (1 old revoked, 1 new active)
        Assert.Equal(2, _refreshTokenStore.RefreshTokens.Count);

        // Verify the old token was successfully revoked
        var oldToken = _refreshTokenStore.RefreshTokens.Single(t => t.Token == expectedOldTokenHash);
        Assert.False(oldToken.IsActive, "The old refresh token must be revoked.");

        // Verify the new token was correctly added
        var newToken = _refreshTokenStore.RefreshTokens.Single(t => t.Token != expectedOldTokenHash);
        Assert.True(newToken.IsActive, "The newly generated refresh token must be active.");
        Assert.Equal(user.Id, newToken.UserId);

        // Verify the hash of the new token was persisted, not the raw string
        Assert.Equal("hashed-new-refresh-token", newToken.Token);

        // Verify database transaction was committed
        Assert.True(
            _unitOfWork.WasSaveChangesCalled,
            "Database transaction must be committed to save the rotation state.");
    }

    [Fact]
    public async Task RefreshAsync_WithNonExistentRefreshToken_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new RefreshTokenRequest(
            "does-not-exist");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            async () =>
                await _sut.RefreshAsync(
                    request,
                    CancellationToken.None));

        Assert.False(
            _tokenService.WasGenerateTokensCalled,
            "Tokens must not be generated for an unknown refresh token.");

        Assert.Empty(
            _refreshTokenStore.AddedRefreshTokens);

        Assert.False(
            _unitOfWork.WasSaveChangesCalled,
            "No persistence should occur for an invalid refresh token.");
    }

    [Fact]
    public async Task RefreshAsync_WithExpiredRefreshToken_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var user = CreateUser(UserRole.Student);

        _userStore.Users.Add(user);

        var storedToken = CreateRefreshToken(
            user.Id,
            "hashed-expired-refresh-token");

        typeof(RefreshToken)
            .GetProperty(nameof(RefreshToken.ExpiresAtUtc))!
            .SetValue(storedToken, DateTime.UtcNow.AddMinutes(-5));

        _refreshTokenStore.RefreshTokens.Add(storedToken);

        var request = new RefreshTokenRequest(
            "expired-refresh-token");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            async () =>
                await _sut.RefreshAsync(
                    request,
                    CancellationToken.None));

        Assert.False(
            _tokenService.WasGenerateTokensCalled,
            "Tokens must not be generated for an expired refresh token.");

        Assert.Empty(
            _refreshTokenStore.AddedRefreshTokens);

        Assert.False(
            _unitOfWork.WasSaveChangesCalled,
            "No persistence should occur for an expired refresh token.");
    }

    [Fact]
    public async Task RefreshAsync_WithRevokedRefreshToken_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var user = CreateUser(UserRole.Student);

        _userStore.Users.Add(user);

        var storedToken = CreateRefreshToken(
            user.Id,
            "hashed-revoked-refresh-token");

        storedToken.Revoke();

        _refreshTokenStore.RefreshTokens.Add(storedToken);

        var request = new RefreshTokenRequest(
            "revoked-refresh-token");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            async () =>
                await _sut.RefreshAsync(
                    request,
                    CancellationToken.None));

        Assert.False(
            _tokenService.WasGenerateTokensCalled,
            "Tokens must not be generated for a revoked refresh token.");

        Assert.Empty(
            _refreshTokenStore.AddedRefreshTokens);

        Assert.False(
            _unitOfWork.WasSaveChangesCalled,
            "No persistence should occur for a revoked refresh token.");
    }

    [Fact]
    public async Task RefreshAsync_WithInactiveUser_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var user = CreateUser(UserRole.Student);

        user.Deactivate();

        _userStore.Users.Add(user);

        var storedToken = CreateRefreshToken(
            user.Id,
            "hashed-valid-refresh-token");

        _refreshTokenStore.RefreshTokens.Add(storedToken);

        var request = new RefreshTokenRequest(
            "valid-refresh-token");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            async () =>
                await _sut.RefreshAsync(
                    request,
                    CancellationToken.None));

        Assert.False(
            _tokenService.WasGenerateTokensCalled,
            "Tokens must not be generated for an inactive user.");

        Assert.Empty(
            _refreshTokenStore.AddedRefreshTokens);

        Assert.False(
            _unitOfWork.WasSaveChangesCalled,
            "No persistence should occur for an inactive user.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task RefreshAsync_WithEmptyOrWhitespaceRefreshToken_ShouldThrowInvalidCredentialsException(
        string refreshToken)
    {
        // Arrange
        var request = new RefreshTokenRequest(
            refreshToken);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            async () =>
                await _sut.RefreshAsync(
                    request,
                    CancellationToken.None));

        Assert.False(
            _refreshTokenHasher.WasHashCalled,
            "An empty refresh token must not be hashed.");

        Assert.False(
            _tokenService.WasGenerateTokensCalled,
            "Tokens must not be generated for an empty refresh token.");

        Assert.Empty(
            _refreshTokenStore.AddedRefreshTokens);

        Assert.False(
            _unitOfWork.WasSaveChangesCalled,
            "No persistence should occur for an empty refresh token.");
    }

    [Fact]
    public async Task RefreshAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () =>
                await _sut.RefreshAsync(
                    null!,
                    CancellationToken.None));

        Assert.False(
            _refreshTokenHasher.WasHashCalled,
            "A null request must not reach token hashing.");

        Assert.False(
            _tokenService.WasGenerateTokensCalled,
            "Tokens must not be generated for a null request.");

        Assert.Empty(
            _refreshTokenStore.AddedRefreshTokens);

        Assert.False(
            _unitOfWork.WasSaveChangesCalled,
            "No persistence should occur for a null request.");
    }

    [Fact]
    public async Task RefreshAsync_WhenRefreshTokenUserDoesNotExist_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var unknownUserId = Guid.NewGuid();

        var storedToken = CreateRefreshToken(
            unknownUserId,
            "hashed-valid-refresh-token");

        _refreshTokenStore.RefreshTokens.Add(storedToken);

        var request = new RefreshTokenRequest(
            "valid-refresh-token");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            async () =>
                await _sut.RefreshAsync(
                    request,
                    CancellationToken.None));

        Assert.False(
            _tokenService.WasGenerateTokensCalled,
            "Tokens must not be generated when the refresh token's user cannot be found.");

        Assert.Empty(
            _refreshTokenStore.AddedRefreshTokens);

        Assert.False(
            _unitOfWork.WasSaveChangesCalled,
            "No persistence should occur when the associated user cannot be found.");
    }

    [Fact]
    public async Task RefreshAsync_WithRevokedRefreshToken_ShouldNotModifyRevokedToken()
    {
        // Arrange
        var user = CreateUser(UserRole.Student);

        _userStore.Users.Add(user);

        var storedToken = CreateRefreshToken(
            user.Id,
            "hashed-revoked-refresh-token");

        storedToken.Revoke();

        var originalRevokedAt = storedToken.RevokedAtUtc;

        _refreshTokenStore.RefreshTokens.Add(storedToken);

        var request = new RefreshTokenRequest(
            "revoked-refresh-token");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            async () =>
                await _sut.RefreshAsync(
                    request,
                    CancellationToken.None));

        Assert.True(storedToken.IsRevoked);

        Assert.Equal(
            originalRevokedAt,
            storedToken.RevokedAtUtc);

        Assert.False(
            _tokenService.WasGenerateTokensCalled);

        Assert.Empty(
            _refreshTokenStore.AddedRefreshTokens);

        Assert.False(
            _unitOfWork.WasSaveChangesCalled);
    }
    #region Test Helpers

    private static User CreateUser(UserRole role)
    {
        var user = new User(
            "student@school.com",
            "HashedSecurePassword",
            role);

        SetEntityId(user);

        return user;
    }

    private static RefreshToken CreateRefreshToken(
        Guid userId,
        string tokenHash,
        TimeSpan? lifetime = null)
    {
        var refreshToken = new RefreshToken(
            userId,
            tokenHash,
            lifetime ?? TimeSpan.FromDays(7));

        SetEntityId(refreshToken);

        return refreshToken;
    }

    private static void SetEntityId(BaseEntity entity)
    {
        typeof(BaseEntity)
            .GetProperty(nameof(BaseEntity.Id))!
            .SetValue(entity, Guid.NewGuid());
    }

    #endregion

    #region Test Doubles

    private class FakeRefreshTokenHasher : IRefreshTokenHasher
    {
        public bool WasHashCalled { get; private set; }

        public string Hash(string refreshToken)
        {
            WasHashCalled = true;

            return $"hashed-{refreshToken}";
        }
    }

    private class FakeRefreshTokenStore : IRefreshTokenStore
    {
        public List<RefreshToken> RefreshTokens { get; } = [];

        public List<RefreshToken> AddedRefreshTokens { get; } = [];

        public Task AddAsync(
            RefreshToken refreshToken,
            CancellationToken cancellationToken = default)
        {
            RefreshTokens.Add(refreshToken);
            AddedRefreshTokens.Add(refreshToken);

            return Task.CompletedTask;
        }

        public Task<RefreshToken?> GetByTokenAsync(
            string tokenHash,
            CancellationToken cancellationToken = default)
        {
            var token =
                RefreshTokens.SingleOrDefault(
                    x => x.Token == tokenHash);

            return Task.FromResult(token);
        }
    }

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
                x => x.Email == normalizedEmail);

            return Task.FromResult(user);
        }

        public Task<User?> GetByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var user = Users.SingleOrDefault(
                x => x.Id == userId);

            return Task.FromResult(user);
        }

        public Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            Users.Add(user);

            return Task.CompletedTask;
        }


    }

    private class FakeTokenService : ITokenService
    {
        public bool WasGenerateTokensCalled { get; private set; }

        public DateTimeOffset ExpectedExpiration { get; } =
            DateTimeOffset.UtcNow.AddMinutes(15);

        public TokenResult GenerateTokens(
            Guid userId,
            string email,
            UserRole role)
        {
            WasGenerateTokensCalled = true;

            return new TokenResult(
                "new-access-token",
                "new-refresh-token",
                ExpectedExpiration);
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

    private static void ForceSetId(object entity, Guid id)
    {
        var type = entity.GetType();

        while (type != null)
        {
            // 1. Try to set the property directly (works if setter is protected/private)
            var property = type.GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (property != null && property.CanWrite)
            {
                property.SetValue(entity, id, null);
                return;
            }

            // 2. Try to set the compiler-generated backing field (works for init-only or restricted setters)
            var field = type.GetField("<Id>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(entity, id);
                return;
            }

            // 3. Move up to the base class (e.g., Entity or BaseEntity) and try again
            type = type.BaseType;
        }

        throw new Exception("Could not find an 'Id' property or backing field on the entity or its base classes.");
    }

    #endregion
}
