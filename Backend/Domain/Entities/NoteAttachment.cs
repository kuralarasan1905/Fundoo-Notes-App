using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fundoo_notes.Domain.Entities
{
    /// <summary>
    /// Note attachment entity for file uploads
    /// </summary>
    public class NoteAttachment : BaseEntity
    {
        [Required]
        public int NoteId { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FileType { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FileUrl { get; set; } = string.Empty;

        public long FileSize { get; set; }

        [StringLength(32)]
        public string? FileHash { get; set; }

        // Navigation properties
        [ForeignKey("NoteId")]
        public virtual Note Note { get; set; } = null!;
    }
}
