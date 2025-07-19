using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fundoo_notes.Domain.Entities
{
    /// <summary>
    /// Junction entity for many-to-many relationship between Notes and Labels
    /// </summary>
    public class NoteLabel : BaseEntity
    {
        // Foreign keys
        [Required]
        public int NoteId { get; set; }
        
        [Required]
        public int LabelId { get; set; }
        
        // Navigation properties
        [ForeignKey("NoteId")]
        public virtual Note Note { get; set; } = null!;
        
        [ForeignKey("LabelId")]
        public virtual Label Label { get; set; } = null!;
    }
}
