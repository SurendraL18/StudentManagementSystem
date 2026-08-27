using StudentManagement.Domain.Common;
using StudentManagement.Domain.Enums;

namespace StudentManagement.Domain.Entities
{
    public class Attendance : BaseEntity
    {
        public Guid StudentId { get; private set; }

        public Guid CourseId { get; private set; }

        public DateOnly AttendanceDate { get; private set; }

        public AttendanceStatus Status { get; private set; }

        public Attendance(Guid studentId, Guid courseId, DateOnly attendanceDate, AttendanceStatus status)
        {
            if (studentId == Guid.Empty)
                throw new ArgumentException("Student Id is Required", nameof(studentId));

            if (courseId == Guid.Empty)
                throw new ArgumentException("Course Id is Required", nameof(courseId));

            if (attendanceDate > DateOnly.FromDateTime(DateTime.UtcNow))
                throw new ArgumentException("Attendance date cannot be in the future.", nameof(attendanceDate));


            StudentId = studentId;
            CourseId = courseId;
            AttendanceDate = attendanceDate;
            Status = status;
        }

        public void MarkPresent()
        {
            Status = AttendanceStatus.Present;
        }

        public void MarkLate()
        {
            Status = AttendanceStatus.Late;
        }

        public void MarkAbsent()
        {
            Status = AttendanceStatus.Absent;
        }


    }
}
