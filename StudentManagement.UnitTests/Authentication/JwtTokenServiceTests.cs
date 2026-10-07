using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using StudentManagement.Domain.Enums;
using StudentManagement.Infrastructure.Security;

namespace StudentManagement.UnitTests.Authentication
{
    public class JwtTokenServiceTests
    {
        [Fact]
        public void GenerateTokens_ShouldCreateValidAccessTokenWithExpectedClaims()
        {
            // 1. Arrange - Setup real configuration using an IOptions wrapper
            var targetSettings = new JwtSettings
            {
                SecretKey = "a_super_secure_test_signing_key_minimum_256_bits_long", // 53 characters
                Issuer = "Test.Issuer.API",
                Audience = "Test.Audience.Client",
                AccessTokenExpirationMinutes = 15
            };

            var optionsWrapper = Options.Create(targetSettings);

            // Instantiate the real concrete production service
            var sut = new JwtTokenService(optionsWrapper);

            var suppliedUserId = Guid.NewGuid();
            const string suppliedEmail = "student@school.com";
            const UserRole suppliedRole = UserRole.Student;

            // 2. Act - Run the cryptographic token creation suite engine
            var result = sut.GenerateTokens(suppliedUserId, suppliedEmail, suppliedRole);

            // 3. Assert - Verify token results are present and populated
            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken), "Access token string should not be empty.");
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken), "Refresh token string should not be empty.");
            Assert.True(result.AccessTokenExpiresAt > DateTimeOffset.UtcNow, "Expiration timestamp must map to a future point-in-time boundary.");

            // 4. Decode the JWT to inspect its claims payload directly
            var tokenHandler = new JwtSecurityTokenHandler();

            Assert.True(tokenHandler.CanReadToken(result.AccessToken), "The produced access token must be a readable RFC compliant JWT format string.");

            var jwtToken = tokenHandler.ReadJwtToken(result.AccessToken);

            // 5. Extract and evaluate target claims statements
            var actualSub = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var actualEmail = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;

            // FIX: Check for the standard JWT "role" key, with a fallback to the Microsoft ClaimTypes URI
            var actualRole = jwtToken.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role)?.Value;

            Assert.Equal(suppliedUserId.ToString(), actualSub);
            Assert.Equal(suppliedEmail, actualEmail);
            Assert.Equal(suppliedRole.ToString(), actualRole);

            // 6. Verify structural token metadata properties match settings config strings
            Assert.Equal(targetSettings.Issuer, jwtToken.Issuer);
            Assert.Contains(targetSettings.Audience, jwtToken.Audiences);

            // Confirm unique tracing JTI exists
            var actualJti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            Assert.NotNull(actualJti);
        }
    }
}
