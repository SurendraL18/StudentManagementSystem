
using StudentManagement.Domain.Common;
using StudentManagement.Domain.Enums;

namespace StudentManagement.Domain.Entities
{
    public class User:BaseEntity
    {
        public string Email { get;private set; }=string.Empty;

        public string PasswordHash { get; private set; } = string.Empty;

        public UserRole Role { get; private set; }

        public UserStatus Status { get; private set; }

        private User() { }

        public User(string email,string passwordHash , UserRole role)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

            Email = email.ToLowerInvariant().Trim();
            PasswordHash = passwordHash;
            Role = role;
            Status = UserStatus.Active;


        }

        public void Deactivate()
        {
            Status = UserStatus.Inactive;
        }

        public void Activate()
        {
            Status = UserStatus.Active;
        }


        public void ChangeRole(UserRole newRole)
        {
            Role = newRole;
        }

    }
}
