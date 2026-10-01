using StudentManagement.Domain.Common;

namespace StudentManagement.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; private set; }

        // Stores the cryptographic hash of the refresh token,
        // never the raw token sent to the client.
        public string Token { get; private set; } = string.Empty;

        public DateTime ExpiresAtUtc { get; private set; }
        public DateTime? RevokedAtUtc { get; private set; }

        private RefreshToken() { }

        public RefreshToken(
            Guid userId,
            string tokenHash,
            TimeSpan lifetime)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException(
                    "User ID cannot be empty.",
                    nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(tokenHash))
            {
                throw new ArgumentException(
                    "Token hash cannot be empty or null.",
                    nameof(tokenHash));
            }

            if (lifetime <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(lifetime),
                    "Refresh token lifetime must be greater than zero.");
            }

            UserId = userId;
            Token = tokenHash;

            var now = DateTime.UtcNow;

            CreatedAtUtc = now;
            UpdatedAtUtc = now;
            ExpiresAtUtc = now.Add(lifetime);
        }

        public bool IsExpired =>
            DateTime.UtcNow >= ExpiresAtUtc;

        public bool IsRevoked =>
            RevokedAtUtc.HasValue;

        public bool IsActive =>
            !IsExpired && !IsRevoked;

        public void Revoke()
        {
            if (IsRevoked)
            {
                return;
            }

            var now = DateTime.UtcNow;

            RevokedAtUtc = now;
            UpdatedAtUtc = now;
        }
    }
}
