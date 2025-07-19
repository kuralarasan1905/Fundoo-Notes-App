using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for NoteAttachment entity
    /// </summary>
    public class NoteAttachmentConfiguration : IEntityTypeConfiguration<NoteAttachment>
    {
        public void Configure(EntityTypeBuilder<NoteAttachment> builder)
        {
            // Table name
            builder.ToTable("NoteAttachments");

            // Primary key
            builder.HasKey(na => na.Id);

            // Properties
            builder.Property(na => na.NoteId)
                .IsRequired();

            builder.Property(na => na.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(na => na.FileType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(na => na.FileUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(na => na.FileSize)
                .IsRequired();

            builder.Property(na => na.FileHash)
                .HasMaxLength(32);

            // Indexes
            builder.HasIndex(na => na.NoteId)
                .HasDatabaseName("IX_NoteAttachments_NoteId");

            builder.HasIndex(na => na.FileHash)
                .HasDatabaseName("IX_NoteAttachments_FileHash");

            // Relationships
            builder.HasOne(na => na.Note)
                .WithMany(n => n.Attachments)
                .HasForeignKey(na => na.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Soft delete filter
            builder.HasQueryFilter(na => !na.IsDeleted);
        }
    }
}
