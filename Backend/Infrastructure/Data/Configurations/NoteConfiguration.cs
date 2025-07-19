using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for Note entity
    /// </summary>
    public class NoteConfiguration : IEntityTypeConfiguration<Note>
    {
        public void Configure(EntityTypeBuilder<Note> builder)
        {
            // Table name
            builder.ToTable("Notes");

            // Primary key
            builder.HasKey(n => n.Id);

            // Properties
            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(n => n.Content)
                .HasMaxLength(5000);

            builder.Property(n => n.Color)
                .HasMaxLength(7); // For hex color codes

            builder.Property(n => n.UserId)
                .IsRequired();

            // Indexes
            builder.HasIndex(n => n.UserId)
                .HasDatabaseName("IX_Notes_UserId");

            builder.HasIndex(n => n.IsPinned)
                .HasDatabaseName("IX_Notes_IsPinned");

            builder.HasIndex(n => n.IsArchived)
                .HasDatabaseName("IX_Notes_IsArchived");

            builder.HasIndex(n => n.IsTrashed)
                .HasDatabaseName("IX_Notes_IsTrashed");

            builder.HasIndex(n => n.ReminderDateTime)
                .HasDatabaseName("IX_Notes_ReminderDateTime");

            builder.HasIndex(n => new { n.UserId, n.CreatedAt })
                .HasDatabaseName("IX_Notes_UserId_CreatedAt");

            // Relationships
            builder.HasOne(n => n.User)
                .WithMany(u => u.Notes)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(n => n.NoteLabels)
                .WithOne(nl => nl.Note)
                .HasForeignKey(nl => nl.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(n => n.Collaborators)
                .WithOne(c => c.Note)
                .HasForeignKey(c => c.NoteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
