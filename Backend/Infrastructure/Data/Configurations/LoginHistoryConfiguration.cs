using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for LoginHistory entity
    /// </summary>
    public class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
    {
        public void Configure(EntityTypeBuilder<LoginHistory> builder)
        {
            // Table name
            builder.ToTable("LoginHistory");

            // Primary key
            builder.HasKey(lh => lh.Id);

            // Properties
            builder.Property(lh => lh.UserId)
                .IsRequired();

            builder.Property(lh => lh.IpAddress)
                .IsRequired()
                .HasMaxLength(45);

            builder.Property(lh => lh.UserAgent)
                .HasMaxLength(500);

            builder.Property(lh => lh.Location)
                .HasMaxLength(100);

            builder.Property(lh => lh.Device)
                .HasMaxLength(50);

            builder.Property(lh => lh.FailureReason)
                .HasMaxLength(200);

            builder.Property(lh => lh.LoginTime)
                .IsRequired();

            // Relationships
            builder.HasOne(lh => lh.User)
                .WithMany()
                .HasForeignKey(lh => lh.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(lh => lh.UserId)
                .HasDatabaseName("IX_LoginHistory_UserId");

            builder.HasIndex(lh => lh.LoginTime)
                .HasDatabaseName("IX_LoginHistory_LoginTime");

            builder.HasIndex(lh => new { lh.UserId, lh.LoginTime })
                .HasDatabaseName("IX_LoginHistory_UserId_LoginTime");
        }
    }
}
