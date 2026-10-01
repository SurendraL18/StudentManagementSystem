using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Domain.Entities;

namespace StudentManagement.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("refresh_tokens");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id");

            builder.Property(x => x.UserId)
                .HasColumnName("UserId")
                .IsRequired();

            builder.Property(x => x.Token)
                .HasColumnName("Token")
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(x => x.Token)
                .IsUnique();

            builder.Property(x => x.ExpiresAtUtc)
                .HasColumnName("ExpiresAtUtc")
                .IsRequired();

            builder.Property(x => x.RevokedAtUtc)
                .HasColumnName("RevokedAtUtc")
                .IsRequired(false);

            builder.Property(x => x.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .IsRequired();

            builder.Property(x => x.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .IsRequired();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
