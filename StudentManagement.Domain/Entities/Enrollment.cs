using StudentManagement.Domain.Common;
using StudentManagement.Domain.Enums;

namespace StudentManagement.Domain.Entities
{
    public class Enrollment : BaseEntity
    {
        public Guid StudentId { get; private set; }

        public Guid CourseId { get; private set; }

        public DateTime EnrollmentDate { get; private set; }

        public EnrollmentStatus Status { get; private set; }

        public Enrollment(Guid studentId, Guid courseId)
        {
            if (studentId == Guid.Empty)
                throw new ArgumentException("Student Id is required", nameof(studentId));

            if (courseId == Guid.Empty)
                throw new ArgumentException("Course Id is required", nameof(courseId));

            StudentId = studentId;
            CourseId = courseId;
            EnrollmentDate = DateTime.UtcNow;
            Status = EnrollmentStatus.Active;
        }

        public void Drop()
        {
            if (Status == EnrollmentStatus.Completed)
                throw new InvalidOperationException("A completed enrollment cannot be dropped.");

            Status = EnrollmentStatus.Dropped;
        }

        public void Complete()
        {
            if (Status != EnrollmentStatus.Active)
                throw new InvalidOperationException(
                    "Only an active enrollment can be completed.");

            Status = EnrollmentStatus.Completed;
        }




    }
}
