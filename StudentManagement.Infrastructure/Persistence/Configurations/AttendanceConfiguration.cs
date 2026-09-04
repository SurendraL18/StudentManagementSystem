using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Domain.Entities;

namespace StudentManagement.Infrastructure.Persistence.Configurations
{
    public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> builder)
        {
            builder.ToTable("attendances");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

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

            builder.Property(x => x.AttendanceDate)
                .IsRequired()
                .HasColumnType("date");


            builder.Property(x => x.Status)
              .IsRequired()
              .HasConversion<string>();

            builder.HasIndex(x => new
            {
                x.StudentId,
                x.CourseId,
                x.AttendanceDate
            })
            .IsUnique();

        }
    }
}
