using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for User entity
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Table name
            builder.ToTable("Users");

            // Primary key
            builder.HasKey(u => u.Id);

            // Properties
            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(u => u.DateOfBirth)
                .HasColumnType("date");

            builder.Property(u => u.Bio)
                .HasMaxLength(500);

            builder.Property(u => u.PasswordHash)
                .IsRequired();

            builder.Property(u => u.EmailVerificationToken)
                .HasMaxLength(500);

            builder.Property(u => u.PasswordResetToken)
                .HasMaxLength(500);

            // Indexes
            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            builder.HasIndex(u => u.EmailVerificationToken)
                .HasDatabaseName("IX_Users_EmailVerificationToken");

            builder.HasIndex(u => u.PasswordResetToken)
                .HasDatabaseName("IX_Users_PasswordResetToken");

            // Relationships
            builder.HasMany(u => u.Notes)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.Labels)
                .WithOne(l => l.User)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
