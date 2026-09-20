using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Domain.Entities;

namespace StudentManagement.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            
            builder.ToTable("users");

           
            builder.HasKey(u => u.Id);

            
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

        
            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(2048);

           
            builder.Property(u => u.Role)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

           
            builder.Property(u => u.CreatedAtUtc)
                 .HasColumnName("created_at_utc")
                .IsRequired();

            builder.Property(u => u.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .IsRequired();
        }
    }
}
