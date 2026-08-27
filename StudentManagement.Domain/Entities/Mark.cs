using StudentManagement.Domain.Common;
using StudentManagement.Domain.Enums;

namespace StudentManagement.Domain.Entities
{
    public class Mark : BaseEntity
    {
        public Guid StudentId { get; private set; }

        public Guid CourseId { get; private set; }

        public AssessmentType AssessmentType { get; private set; }

        public decimal Score { get; private set; }

        public decimal MaxScore { get; private set; }

        public decimal Percentage => MaxScore == 0 ? 0 : Score / MaxScore * 100;

        public Mark(Guid studentId, Guid courseId, AssessmentType assessmentType, decimal score, decimal maxScore)
        {

            if (studentId == Guid.Empty)
                throw new ArgumentException("Student Id is required", nameof(studentId));

            if (courseId == Guid.Empty)
                throw new ArgumentException("Course Id is required", nameof(courseId));

            ValidateScore(score, maxScore);

            StudentId = studentId;
            CourseId = courseId;
            AssessmentType = assessmentType;
            Score = score;
            MaxScore = maxScore;
        }

        public void UpdateScore(decimal score)
        {
            ValidateScore(score, MaxScore);

            Score = score;
        }

        private static void ValidateScore(
            decimal score,
            decimal maxScore)
        {
            if (maxScore <= 0)
                throw new ArgumentException(
                    "Maximum score must be greater than zero.",
                    nameof(maxScore));

            if (score < 0)
                throw new ArgumentException(
                    "Score cannot be negative.",
                    nameof(score));

            if (score > maxScore)
                throw new ArgumentException(
                    "Score cannot be greater than the maximum score.",
                    nameof(score));
        }
    }
}
