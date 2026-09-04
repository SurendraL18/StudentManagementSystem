using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Domain.Entities;

namespace StudentManagement.Infrastructure.Persistence.Configurations
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.ToTable("enrollments");

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


            builder.Property(x => x.EnrollmentDate)
              .IsRequired()
              .HasColumnType("timestamp with time zone");


            builder.Property(x => x.Status)
              .IsRequired()
              .HasConversion<string>();


        }
    }
}
