using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for NoteLabel entity
    /// </summary>
    public class NoteLabelConfiguration : IEntityTypeConfiguration<NoteLabel>
    {
        public void Configure(EntityTypeBuilder<NoteLabel> builder)
        {
            // Table name
            builder.ToTable("NoteLabels");

            // Primary key
            builder.HasKey(nl => nl.Id);

            // Properties
            builder.Property(nl => nl.NoteId)
                .IsRequired();

            builder.Property(nl => nl.LabelId)
                .IsRequired();

            // Indexes
            builder.HasIndex(nl => nl.NoteId)
                .HasDatabaseName("IX_NoteLabels_NoteId");

            builder.HasIndex(nl => nl.LabelId)
                .HasDatabaseName("IX_NoteLabels_LabelId");

            builder.HasIndex(nl => new { nl.NoteId, nl.LabelId })
                .IsUnique()
                .HasDatabaseName("IX_NoteLabels_NoteId_LabelId");

            // Relationships
            builder.HasOne(nl => nl.Note)
                .WithMany(n => n.NoteLabels)
                .HasForeignKey(nl => nl.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(nl => nl.Label)
                .WithMany(l => l.NoteLabels)
                .HasForeignKey(nl => nl.LabelId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
