using StudentManagement.Domain.Common;

namespace StudentManagement.Domain.Entities
{
    public class Student : BaseEntity
    {
        public string StudentNumber { get; private set; }

        public string FirstName { get; private set; }

        public string LastName { get; private set; }

        public string Email { get; private set; }

        public DateTime DateOfBirth { get; private set; }

        public bool IsActive { get; private set; }

        public Student(
            string studentNumber,
            string firstName,
            string lastName,
            string email,
            DateTime dateOfBirth)
        {
            if (string.IsNullOrWhiteSpace(studentNumber))
                throw new ArgumentException("Student Number is Required", nameof(studentNumber));

            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required.", nameof(firstName));

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name is required.", nameof(lastName));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));
            if (dateOfBirth > DateTime.UtcNow.Date)

                throw new ArgumentException(
                    "Date of birth cannot be in the future.",
                    nameof(dateOfBirth));

            StudentNumber = studentNumber.Trim();
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            Email = email.Trim();
            DateOfBirth = dateOfBirth;
            IsActive = true;
        }

        public void UpdateName(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First Name is Required", nameof(firstName));

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last Name is Rrquired", nameof(lastName));

            FirstName = firstName.Trim();
            LastName = lastName.Trim();

        }

        public void UpdateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required", nameof(email));

            Email = email.Trim();

        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }

    }
}
