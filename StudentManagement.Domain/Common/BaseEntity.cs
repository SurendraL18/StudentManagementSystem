namespace StudentManagement.Domain.Common
{
    public abstract class BaseEntity
    {

        public Guid Id { get; private set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
    }
}
