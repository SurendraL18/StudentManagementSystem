using Microsoft.AspNetCore.Identity;
using StudentManagement.Application.Common.Interfaces;

namespace StudentManagement.Infrastructure.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<string> _microsoftHasher = new();
        private const string dummyUserContext = "";
        public string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password Cannot be empty", nameof(password));
            }

            return _microsoftHasher.HashPassword(dummyUserContext, password);
        }

        public bool Verify(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
            {
                return false;
            }

            var result = _microsoftHasher.VerifyHashedPassword(dummyUserContext, hashedPassword, password);

            return result == PasswordVerificationResult.Success ||
                   result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
