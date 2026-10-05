using StudentManagement.Application.Common.Interfaces;
using StudentManagement.Infrastructure.Security;

namespace StudentManagement.UnitTests.Authentication
{
    public class RefreshTokenHasherTests
    {
        private readonly IRefreshTokenHasher _sut;

        public RefreshTokenHasherTests()
        {
            _sut = new RefreshTokenHasher();
        }

        [Fact]
        public void Hash_WithValidRefreshToken_ShouldReturn64CharacterHash()
        {
            // Arrange
            var refreshToken = "valid-refresh-token-value";

            // Act
            var result = _sut.Hash(refreshToken);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(64, result.Length);
        }

        [Fact]
        public void Hash_WithSameRefreshToken_ShouldReturnSameHash()
        {
            // Arrange
            var refreshToken = "same-refresh-token-value";

            // Act
            var firstHash = _sut.Hash(refreshToken);
            var secondHash = _sut.Hash(refreshToken);

            // Assert
            Assert.Equal(firstHash, secondHash);
        }

        [Fact]
        public void Hash_WithDifferentRefreshTokens_ShouldReturnDifferentHashes()
        {
            // Arrange
            var firstRefreshToken = "first-refresh-token-value";
            var secondRefreshToken = "second-refresh-token-value";

            // Act
            var firstHash = _sut.Hash(firstRefreshToken);
            var secondHash = _sut.Hash(secondRefreshToken);

            // Assert
            Assert.NotEqual(firstHash, secondHash);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void Hash_WithEmptyOrWhitespaceRefreshToken_ShouldThrowArgumentException(
            string refreshToken)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => _sut.Hash(refreshToken));
        }

        [Fact]
        public void Hash_ShouldReturnLowercaseHexadecimalHash()
        {
            // Arrange
            var refreshToken = "refresh-token-for-format-validation";

            // Act
            var result = _sut.Hash(refreshToken);

            // Assert
            Assert.Equal(
                result.ToLowerInvariant(),
                result);

            Assert.All(
                result,
                character =>
                    Assert.True(
                        Uri.IsHexDigit(character),
                        $"Character '{character}' is not a valid hexadecimal character."));
        }
    }
}
