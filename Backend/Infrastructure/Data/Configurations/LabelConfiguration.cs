using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for Label entity
    /// </summary>
    public class LabelConfiguration : IEntityTypeConfiguration<Label>
    {
        public void Configure(EntityTypeBuilder<Label> builder)
        {
            // Table name
            builder.ToTable("Labels");

            // Primary key
            builder.HasKey(l => l.Id);

            // Properties
            builder.Property(l => l.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(l => l.Color)
                .HasMaxLength(7); // For hex color codes

            builder.Property(l => l.UserId)
                .IsRequired();

            // Indexes
            builder.HasIndex(l => l.UserId)
                .HasDatabaseName("IX_Labels_UserId");

            builder.HasIndex(l => new { l.UserId, l.Name })
                .IsUnique()
                .HasDatabaseName("IX_Labels_UserId_Name");

            // Relationships
            builder.HasOne(l => l.User)
                .WithMany(u => u.Labels)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(l => l.NoteLabels)
                .WithOne(nl => nl.Label)
                .HasForeignKey(nl => nl.LabelId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
