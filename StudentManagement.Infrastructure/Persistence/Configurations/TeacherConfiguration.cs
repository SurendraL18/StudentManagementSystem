using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Domain.Entities;

namespace StudentManagement.Infrastructure.Persistence.Configurations
{
    public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
    {
        public void Configure(EntityTypeBuilder<Teacher> builder)
        {
            builder.ToTable("teachers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.EmployeeNumber)
              .IsRequired()
              .HasMaxLength(50);

            builder.HasIndex(x => x.EmployeeNumber)
             .IsUnique();

            builder.Property(x => x.FirstName)
             .IsRequired()
             .HasMaxLength(100);

            builder.Property(x => x.LastName)
             .IsRequired()
             .HasMaxLength(100);

            builder.Property(x => x.Email)
             .IsRequired()
             .HasMaxLength(320);

            builder.HasIndex(x => x.Email)
              .IsUnique();

            builder.Property(x => x.Department)
             .IsRequired()
             .HasMaxLength(150);

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
