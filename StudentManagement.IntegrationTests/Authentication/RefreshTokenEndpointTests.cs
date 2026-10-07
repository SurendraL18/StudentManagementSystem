using System.Net;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Application.Authentication;

namespace StudentManagement.IntegrationTests.Authentication;

public class RefreshTokenEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RefreshTokenEndpointTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Refresh_ShouldReturn401Unauthorized_WhenTokenIsInvalid()
    {
        // Arrange
        var requestPayload =
            new RefreshTokenRequest("invalid-refresh-token");

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/Authentication/refresh",
                requestPayload);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var problem =
            await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);

        Assert.Equal(
            "/problems/invalid-credentials",
            problem.Type);

        Assert.Equal(
            (int)HttpStatusCode.Unauthorized,
            problem.Status);
    }

    [Fact]
    public async Task Refresh_ShouldReturnNewTokens_WhenTokenIsValid()
    {
        // Arrange
        var email =
            $"refresh-success.{Guid.NewGuid()}@school.com";

        var password =
            "SecurePassword123!";

        // Create user using the existing user endpoint.
        var createUserPayload = new
        {
            Email = email,
            Password = password,
            Role = "Student"
        };

        var createUserResponse =
            await _client.PostAsJsonAsync(
                "/api/Users",
                createUserPayload);

        createUserResponse.EnsureSuccessStatusCode();

        // Login.
        var loginRequest =
            new LoginRequest(
                email,
                password);

        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/Authentication/login",
                loginRequest);

        loginResponse.EnsureSuccessStatusCode();

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(loginResult);
        Assert.NotEmpty(loginResult.RefreshToken);

        var originalRefreshToken =
            loginResult.RefreshToken;

        var originalAccessToken =
            loginResult.AccessToken;

        // Refresh.
        var refreshRequest =
            new RefreshTokenRequest(
                originalRefreshToken);

        // Act
        var refreshResponse =
            await _client.PostAsJsonAsync(
                "/api/Authentication/refresh",
                refreshRequest);

        // Assert
        refreshResponse.EnsureSuccessStatusCode();

        var refreshResult =
            await refreshResponse.Content
                .ReadFromJsonAsync<RefreshTokenResponse>();

        Assert.NotNull(refreshResult);

        Assert.NotEmpty(
            refreshResult.AccessToken);

        Assert.NotEmpty(
            refreshResult.RefreshToken);

        Assert.NotEqual(
            originalAccessToken,
            refreshResult.AccessToken);

        Assert.NotEqual(
            originalRefreshToken,
            refreshResult.RefreshToken);

        Assert.True(
            refreshResult.AccessTokenExpiresAt >
            DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Refresh_ShouldReturn401Unauthorized_WhenRefreshTokenIsUsedTwice()
    {
        // Arrange
        var email =
            $"refresh-reuse.{Guid.NewGuid()}@school.com";

        var password =
            "SecurePassword123!";

        var createUserPayload = new
        {
            Email = email,
            Password = password,
            Role = "Student"
        };

        var createUserResponse =
            await _client.PostAsJsonAsync(
                "/api/Users",
                createUserPayload);

        createUserResponse.EnsureSuccessStatusCode();

        var loginRequest =
            new LoginRequest(
                email,
                password);

        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/Authentication/login",
                loginRequest);

        loginResponse.EnsureSuccessStatusCode();

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(loginResult);

        var originalRefreshToken =
            loginResult.RefreshToken;

        var refreshRequest =
            new RefreshTokenRequest(
                originalRefreshToken);

        // First refresh succeeds and revokes the old token.
        var firstRefreshResponse =
            await _client.PostAsJsonAsync(
                "/api/Authentication/refresh",
                refreshRequest);

        firstRefreshResponse.EnsureSuccessStatusCode();

        // Act
        // Reuse the OLD refresh token.
        var secondRefreshResponse =
            await _client.PostAsJsonAsync(
                "/api/Authentication/refresh",
                refreshRequest);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            secondRefreshResponse.StatusCode);

        var problem =
            await secondRefreshResponse.Content
                .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);

        Assert.Equal(
            "/problems/invalid-credentials",
            problem.Type);
    }
}
