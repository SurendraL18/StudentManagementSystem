using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Domain.Entities;

namespace StudentManagement.Infrastructure.Persistence.Configurations
{
    public class MarkConfiguration : IEntityTypeConfiguration<Mark>
    {
        public void Configure(EntityTypeBuilder<Mark> builder)
        {
            builder.ToTable("marks");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.StudentId)
             .IsRequired();

            builder.HasOne<Student>()
             .WithMany()
             .HasForeignKey(x => x.StudentId)
             .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.CourseId)
             .IsRequired();

            builder.HasOne<Course>()
             .WithMany()
             .HasForeignKey(x => x.CourseId)
             .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.AssessmentType)
             .IsRequired()
             .HasConversion<string>();

            builder.Property(x => x.Score)
           .IsRequired()
           .HasColumnType("decimal(5,2)");

            builder.Property(x => x.MaxScore)
           .IsRequired()
           .HasColumnType("decimal(5,2)");

            builder.Ignore(x => x.Percentage);

            builder.HasIndex(x => new
            {
                x.StudentId,
                x.CourseId,
                x.AssessmentType
            }).IsUnique();

        }
    }
}
