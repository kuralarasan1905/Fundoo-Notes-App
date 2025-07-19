using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for NoteHistory entity
    /// </summary>
    public class NoteHistoryConfiguration : IEntityTypeConfiguration<NoteHistory>
    {
        public void Configure(EntityTypeBuilder<NoteHistory> builder)
        {
            // Table name
            builder.ToTable("NoteHistory");

            // Primary key
            builder.HasKey(nh => nh.Id);

            // Properties
            builder.Property(nh => nh.NoteId)
                .IsRequired();

            builder.Property(nh => nh.UserId)
                .IsRequired();

            builder.Property(nh => nh.Action)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(nh => nh.PreviousTitle)
                .HasMaxLength(200);

            builder.Property(nh => nh.PreviousContent)
                .HasMaxLength(5000);

            builder.Property(nh => nh.NewTitle)
                .HasMaxLength(200);

            builder.Property(nh => nh.NewContent)
                .HasMaxLength(5000);

            builder.Property(nh => nh.PreviousColor)
                .HasMaxLength(7);

            builder.Property(nh => nh.NewColor)
                .HasMaxLength(7);

            builder.Property(nh => nh.ChangeDetails)
                .HasMaxLength(1000);

            builder.Property(nh => nh.ActionDateTime)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(nh => nh.NoteId)
                .HasDatabaseName("IX_NoteHistory_NoteId");

            builder.HasIndex(nh => nh.UserId)
                .HasDatabaseName("IX_NoteHistory_UserId");

            builder.HasIndex(nh => nh.ActionDateTime)
                .HasDatabaseName("IX_NoteHistory_ActionDateTime");

            builder.HasIndex(nh => new { nh.NoteId, nh.ActionDateTime })
                .HasDatabaseName("IX_NoteHistory_NoteId_ActionDateTime");

            // Relationships
            builder.HasOne(nh => nh.Note)
                .WithMany(n => n.History)
                .HasForeignKey(nh => nh.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(nh => nh.User)
                .WithMany()
                .HasForeignKey(nh => nh.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Soft delete filter
            builder.HasQueryFilter(nh => !nh.IsDeleted);
        }
    }
}
