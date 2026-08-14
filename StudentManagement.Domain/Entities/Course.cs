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

            throw new ArgumentException("Code is Required", nameof(code));

        if (string.IsNullOrWhiteSpace(title))

            throw new ArgumentException("Title is Required", nameof(title));

        if (capacity <= 0)

            throw new ArgumentException("Capacity should be more than zero", nameof(capacity));




        Code = code.Trim();
        Title = title.Trim();
        Description = description?.Trim();
        Capacity = capacity;
        IsActive = true;
    }

    public void UpdateDetails(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is Required", nameof(title));



        Title = title.Trim();
        Description = description?.Trim();


    }

    public void ChangeCapacity(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Course capacity must be greater than zero", nameof(capacity));

        Capacity = capacity;
    }

    public void AssignTeacher(Guid teacherId)
    {
        if (teacherId == Guid.Empty)
        {
            throw new ArgumentException("Teacher is Required", nameof(teacherId));
        }
        TeacherId = teacherId;
    }

    public void RemoveTeacher()
    {
        TeacherId = null;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }




}
