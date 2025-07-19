using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fundoo_notes.Domain.Entities
{
    /// <summary>
    /// Note history entity for tracking changes
    /// </summary>
    public class NoteHistory : BaseEntity
    {
        [Required]
        public int NoteId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // Created, Updated, Deleted, Restored, Archived, etc.

        [StringLength(200)]
        public string? PreviousTitle { get; set; }

        [StringLength(5000)]
        public string? PreviousContent { get; set; }

        [StringLength(200)]
        public string? NewTitle { get; set; }

        [StringLength(5000)]
        public string? NewContent { get; set; }

        [StringLength(7)]
        public string? PreviousColor { get; set; }

        [StringLength(7)]
        public string? NewColor { get; set; }

        [StringLength(1000)]
        public string? ChangeDetails { get; set; }

        public DateTime ActionDateTime { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("NoteId")]
        public virtual Note Note { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
