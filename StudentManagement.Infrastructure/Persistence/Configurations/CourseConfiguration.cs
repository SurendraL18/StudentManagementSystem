using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Domain.Entities;

namespace StudentManagement.Infrastructure.Persistence.Configurations
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.ToTable("courses");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Code)
              .IsRequired()
              .HasMaxLength(50);
            builder.HasIndex(x => x.Code)
              .IsUnique();

            builder.Property(x => x.Title)
             .IsRequired()
             .HasMaxLength(200);


            builder.Property(x => x.Description)
             .IsRequired(false);

            builder.Property(x => x.Capacity)
             .IsRequired();

            builder.Property(x => x.TeacherId)
             .IsRequired(false);

            builder.HasOne<Teacher>()
                .WithMany()
                .HasForeignKey(x => x.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.IsActive)
             .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
             .HasColumnName("created_at_utc")
             .IsRequired();

            builder.Property(x => x.UpdatedAtUtc)
              .HasColumnName("updated_at_utc")
              .IsRequired();

        }
    }
}
