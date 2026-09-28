namespace StudentManagement.Application.Common.Exceptions
{
    public class DuplicateUserEmailException : Exception
    {
        public string Email { get; }

        public DuplicateUserEmailException(string email)
            : base($"A user with the email '{email}' already exists.")
        {
            Email = email.Trim().ToLowerInvariant();
        }
    }
}
