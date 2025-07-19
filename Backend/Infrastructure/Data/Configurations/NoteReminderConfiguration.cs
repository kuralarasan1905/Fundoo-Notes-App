using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for NoteReminder entity
    /// </summary>
    public class NoteReminderConfiguration : IEntityTypeConfiguration<NoteReminder>
    {
        public void Configure(EntityTypeBuilder<NoteReminder> builder)
        {
            // Table name
            builder.ToTable("NoteReminders");

            // Primary key
            builder.HasKey(nr => nr.Id);

            // Properties
            builder.Property(nr => nr.NoteId)
                .IsRequired();

            builder.Property(nr => nr.ReminderDateTime)
                .IsRequired();

            builder.Property(nr => nr.ReminderType)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Once");

            builder.Property(nr => nr.IsCompleted)
                .HasDefaultValue(false);

            builder.Property(nr => nr.Notes)
                .HasMaxLength(500);

            // Indexes
            builder.HasIndex(nr => nr.NoteId)
                .HasDatabaseName("IX_NoteReminders_NoteId");

            builder.HasIndex(nr => nr.ReminderDateTime)
                .HasDatabaseName("IX_NoteReminders_ReminderDateTime");

            builder.HasIndex(nr => new { nr.IsCompleted, nr.ReminderDateTime })
                .HasDatabaseName("IX_NoteReminders_IsCompleted_ReminderDateTime");

            // Relationships
            builder.HasOne(nr => nr.Note)
                .WithMany(n => n.Reminders)
                .HasForeignKey(nr => nr.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Soft delete filter
            builder.HasQueryFilter(nr => !nr.IsDeleted);
        }
    }
}
