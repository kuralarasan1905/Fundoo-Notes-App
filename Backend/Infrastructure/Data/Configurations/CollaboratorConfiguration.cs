using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for Collaborator entity
    /// </summary>
    public class CollaboratorConfiguration : IEntityTypeConfiguration<Collaborator>
    {
        public void Configure(EntityTypeBuilder<Collaborator> builder)
        {
            // Table name
            builder.ToTable("Collaborators");

            // Primary key
            builder.HasKey(c => c.Id);

            // Properties
            builder.Property(c => c.NoteId)
                .IsRequired();

            builder.Property(c => c.UserId)
                .IsRequired();

            builder.Property(c => c.Permission)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Read");

            // Indexes
            builder.HasIndex(c => c.NoteId)
                .HasDatabaseName("IX_Collaborators_NoteId");

            builder.HasIndex(c => c.UserId)
                .HasDatabaseName("IX_Collaborators_UserId");

            builder.HasIndex(c => new { c.NoteId, c.UserId })
                .IsUnique()
                .HasDatabaseName("IX_Collaborators_NoteId_UserId");

            // Relationships
            builder.HasOne(c => c.Note)
                .WithMany(n => n.Collaborators)
                .HasForeignKey(c => c.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete to avoid cycles
        }
    }
}
