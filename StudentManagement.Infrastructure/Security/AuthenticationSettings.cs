using Microsoft.Extensions.Options;
using StudentManagement.Application.Common.Interfaces;

namespace StudentManagement.Infrastructure.Security
{
    public class AuthenticationSettings : IAuthenticationSettings
    {
        public int RefreshTokenExpirationDays { get; }

        public AuthenticationSettings(IOptions<JwtSettings> jwtOptions)
        {
            if (jwtOptions == null)
            {
                throw new ArgumentNullException(nameof(jwtOptions));
            }

            var settings = jwtOptions.Value;

            if (settings == null)
            {
                throw new InvalidOperationException("JWT settings configuration section is missing.");
            }

            // Validate that the configured lifetime is greater than zero
            if (settings.RefreshTokenExpirationDays <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(settings.RefreshTokenExpirationDays),
                    "The configured RefreshTokenExpirationDays must be greater than zero.");
            }

            RefreshTokenExpirationDays = settings.RefreshTokenExpirationDays;
        }
    }
}
