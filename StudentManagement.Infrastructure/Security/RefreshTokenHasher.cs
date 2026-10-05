using System.Security.Cryptography;
using System.Text;
using StudentManagement.Application.Common.Interfaces;

namespace StudentManagement.Infrastructure.Security
{
    public class RefreshTokenHasher : IRefreshTokenHasher
    {
        public string Hash(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentException(
                    "Refresh token cannot be null, empty, or whitespace.",
                    nameof(refreshToken));
            }

            byte[] tokenBytes = Encoding.UTF8.GetBytes(refreshToken);
            byte[] hashBytes = SHA256.HashData(tokenBytes);

            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}
