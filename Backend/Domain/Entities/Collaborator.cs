using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fundoo_notes.Domain.Entities
{
    /// <summary>
    /// Collaborator entity for note sharing functionality
    /// </summary>
    public class Collaborator : BaseEntity
    {
        // Foreign keys
        [Required]
        public int NoteId { get; set; }
        
        [Required]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Permission { get; set; } = "Read"; // Read, Write, Owner
        
        public bool IsAccepted { get; set; } = false;
        
        public DateTime? AcceptedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("NoteId")]
        public virtual Note Note { get; set; } = null!;
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
