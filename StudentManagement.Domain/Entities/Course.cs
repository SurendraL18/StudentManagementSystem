using StudentManagement.Domain.Common;

namespace StudentManagement.Domain.Entities;

public class Course : BaseEntity
{
    public string Code { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public int Capacity { get; private set; }
    public Guid? TeacherId { get; private set; }
    public bool IsActive { get; private set; }

    public Course(string code, string title, string? description, int capacity)
    {
        if (string.IsNullOrWhiteSpace(code))

            throw new ArgumentNullException("Code is Required", nameof(code));

        if (string.IsNullOrWhiteSpace(title))

            throw new ArgumentNullException("Code is Required", nameof(title));

        if (capacity <= 0)

            throw new ArgumentNullException("Capacity should be more than zero", nameof(capacity));




        Code = code.Trim();
        Title = title.Trim();
        Description = description?.Trim();
        Capacity = capacity;
        IsActive = true;
    }

    public void UpdateDetails(string code)
    {
        Code = code ?? throw new ArgumentNullException("Code is Required", nameof(code));
    }




}
