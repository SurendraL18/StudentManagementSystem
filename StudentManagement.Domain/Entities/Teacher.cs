using StudentManagement.Domain.Common;

namespace StudentManagement.Domain.Entities
{
    public class Teacher : BaseEntity
    {
        public string EmployeeNumber { get; private set; }

        public string FirstName { get; private set; }

        public string LastName { get; private set; }

        public string Email { get; private set; }

        public string Department { get; private set; }

        public bool IsActive { get; private set; }


        public Teacher(
            string employeeNumber,
            string firstName,
            string lastName,
            string email,
            string department)
        {
            if (string.IsNullOrWhiteSpace(employeeNumber))
                throw new ArgumentException("Employee Number is Required", (nameof(employeeNumber)));
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First Name is Required", (nameof(firstName)));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last Name is Required", (nameof(lastName)));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is Required", (nameof(email)));
            if (string.IsNullOrWhiteSpace(department))
                throw new ArgumentException("Department is Required", (nameof(department)));

            EmployeeNumber = employeeNumber.Trim();
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            Email = email.Trim();
            Department = department.Trim();
            IsActive = true;
        }

        public void UpdateName(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First Name is Required", (nameof(firstName)));

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("First Name is Required", (nameof(lastName)));

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
        }

        public void UpdateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is Required", nameof(email));

            Email = email.Trim();
        }

        public void UpdateDepartment(string department)
        {
            if (string.IsNullOrWhiteSpace(department))
                throw new ArgumentException("Department is required", (nameof(department)));

            Department = department.Trim();

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
