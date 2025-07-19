using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fundoo_notes.Domain.Entities
{
    /// <summary>
    /// Label entity for categorizing notes
    /// </summary>
    public class Label : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(7)]
        public string? Color { get; set; }
        
        // Foreign key
        [Required]
        public int UserId { get; set; }
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        
        public virtual ICollection<NoteLabel> NoteLabels { get; set; } = new List<NoteLabel>();
    }
}
