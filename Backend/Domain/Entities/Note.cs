using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fundoo_notes.Domain.Entities
{
    /// <summary>
    /// Note entity representing user notes
    /// </summary>
    public class Note : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(5000)]
        public string Content { get; set; } = string.Empty;

        [StringLength(50)] // Increased to accommodate pattern names like "grid", "lines", "dots", "gradient"
        public string? Color { get; set; }
        
        public bool IsPinned { get; set; } = false;
        
        public bool IsArchived { get; set; } = false;
        
        public bool IsTrashed { get; set; } = false;
        
        public DateTime? ReminderDateTime { get; set; }
        
        // Foreign key
        [Required]
        public int UserId { get; set; }
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        
        public virtual ICollection<NoteLabel> NoteLabels { get; set; } = new List<NoteLabel>();

        public virtual ICollection<Collaborator> Collaborators { get; set; } = new List<Collaborator>();

        public virtual ICollection<NoteAttachment> Attachments { get; set; } = new List<NoteAttachment>();

        public virtual ICollection<NoteReminder> Reminders { get; set; } = new List<NoteReminder>();

        public virtual ICollection<NoteListItem> ListItems { get; set; } = new List<NoteListItem>();

        public virtual ICollection<NoteHistory> History { get; set; } = new List<NoteHistory>();

        // Computed properties
        public bool HasReminder => ReminderDateTime.HasValue && ReminderDateTime > DateTime.UtcNow;

        public string PreviewContent => Content.Length > 100 ? Content.Substring(0, 100) + "..." : Content;

        public bool IsListNote => ListItems.Any();

        public int CompletedItemsCount => ListItems.Count(li => li.IsCompleted);

        public int TotalItemsCount => ListItems.Count;
    }
}
