using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fundoo_notes.Domain.Entities
{
    /// <summary>
    /// Note list item entity for checklist functionality
    /// </summary>
    public class NoteListItem : BaseEntity
    {
        [Required]
        public int NoteId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Text { get; set; } = string.Empty;

        public bool IsCompleted { get; set; } = false;

        public int Order { get; set; } = 0;

        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        [ForeignKey("NoteId")]
        public virtual Note Note { get; set; } = null!;
    }
}
