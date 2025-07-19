using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fundoo_notes.Domain.Entities
{
    /// <summary>
    /// Note reminder entity for enhanced reminder functionality
    /// </summary>
    public class NoteReminder : BaseEntity
    {
        [Required]
        public int NoteId { get; set; }

        [Required]
        public DateTime ReminderDateTime { get; set; }

        [Required]
        [StringLength(20)]
        public string ReminderType { get; set; } = "Once"; // Once, Daily, Weekly, Monthly, Yearly

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        public DateTime? LastTriggeredAt { get; set; }

        public DateTime? NextTriggerAt { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("NoteId")]
        public virtual Note Note { get; set; } = null!;
    }
}
