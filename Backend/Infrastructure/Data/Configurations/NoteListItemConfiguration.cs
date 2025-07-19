using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for NoteListItem entity
    /// </summary>
    public class NoteListItemConfiguration : IEntityTypeConfiguration<NoteListItem>
    {
        public void Configure(EntityTypeBuilder<NoteListItem> builder)
        {
            // Table name
            builder.ToTable("NoteListItems");

            // Primary key
            builder.HasKey(nli => nli.Id);

            // Properties
            builder.Property(nli => nli.NoteId)
                .IsRequired();

            builder.Property(nli => nli.Text)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(nli => nli.IsCompleted)
                .HasDefaultValue(false);

            builder.Property(nli => nli.Order)
                .HasDefaultValue(0);

            // Indexes
            builder.HasIndex(nli => nli.NoteId)
                .HasDatabaseName("IX_NoteListItems_NoteId");

            builder.HasIndex(nli => new { nli.NoteId, nli.Order })
                .HasDatabaseName("IX_NoteListItems_NoteId_Order");

            // Relationships
            builder.HasOne(nli => nli.Note)
                .WithMany(n => n.ListItems)
                .HasForeignKey(nli => nli.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Soft delete filter
            builder.HasQueryFilter(nli => !nli.IsDeleted);
        }
    }
}
