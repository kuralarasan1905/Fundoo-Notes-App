using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for NoteTemplate entity
    /// </summary>
    public class NoteTemplateConfiguration : IEntityTypeConfiguration<NoteTemplate>
    {
        public void Configure(EntityTypeBuilder<NoteTemplate> builder)
        {
            // Table name
            builder.ToTable("NoteTemplates");

            // Primary key
            builder.HasKey(nt => nt.Id);

            // Properties
            builder.Property(nt => nt.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(nt => nt.Description)
                .HasMaxLength(500);

            builder.Property(nt => nt.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(nt => nt.Content)
                .HasMaxLength(5000);

            builder.Property(nt => nt.Color)
                .HasMaxLength(7);

            builder.Property(nt => nt.Category)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(nt => nt.IsPublic)
                .HasDefaultValue(false);

            builder.Property(nt => nt.UsageCount)
                .HasDefaultValue(0);

            // Indexes
            builder.HasIndex(nt => nt.UserId)
                .HasDatabaseName("IX_NoteTemplates_UserId");

            builder.HasIndex(nt => nt.Category)
                .HasDatabaseName("IX_NoteTemplates_Category");

            builder.HasIndex(nt => nt.IsPublic)
                .HasDatabaseName("IX_NoteTemplates_IsPublic");

            builder.HasIndex(nt => new { nt.UserId, nt.Name })
                .HasDatabaseName("IX_NoteTemplates_UserId_Name");

            // Relationships
            builder.HasOne(nt => nt.User)
                .WithMany()
                .HasForeignKey(nt => nt.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Soft delete filter
            builder.HasQueryFilter(nt => !nt.IsDeleted);
        }
    }
}
