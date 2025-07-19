using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fundoo_notes.Domain.Entities
{
    /// <summary>
    /// Note template entity for predefined note formats
    /// </summary>
    public class NoteTemplate : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(5000)]
        public string Content { get; set; } = string.Empty;

        [StringLength(50)] // Allow for pattern names
        public string? Color { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // Personal, Work, Shopping, Travel, etc.

        public bool IsPublic { get; set; } = false;

        public int UsageCount { get; set; } = 0;

        // Foreign key - null for system templates
        public int? UserId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
