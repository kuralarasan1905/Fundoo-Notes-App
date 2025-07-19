using System.ComponentModel.DataAnnotations;

namespace fundoo_notes.Application.DTOs
{
    public class CollaboratorDto
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Permission { get; set; } = string.Empty;
        public bool IsAccepted { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AddCollaboratorDto
    {
        [Required]
        public int NoteId { get; set; }

        [Required]
        [EmailAddress]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        public string Permission { get; set; } = "Read"; // Read, Write, Owner
    }

    public class UpdateCollaboratorPermissionDto
    {
        [Required]
        public string Permission { get; set; } = string.Empty;
    }

    public class NoteAttachmentDto
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateNoteAttachmentDto
    {
        [Required]
        public int NoteId { get; set; }

        [Required]
        public IFormFile File { get; set; } = null!;
    }

    public class NoteReminderDto
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public DateTime ReminderDateTime { get; set; }
        public string ReminderType { get; set; } = "Once"; // Once, Daily, Weekly, Monthly
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        // Note details for frontend display
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Color { get; set; }
    }

    public class CreateNoteReminderDto
    {
        [Required]
        public int NoteId { get; set; }

        [Required]
        public DateTime ReminderDateTime { get; set; }

        public string ReminderType { get; set; } = "Once";
    }

    public class CreateReminderDto
    {
        [Required]
        public int NoteId { get; set; }

        [Required]
        public DateTime ReminderDateTime { get; set; }

        public string ReminderType { get; set; } = "Once";
    }

    public class UpdateNoteReminderDto
    {
        [Required]
        public DateTime ReminderDateTime { get; set; }

        public string ReminderType { get; set; } = "Once";
    }

    public class NoteListItemDto
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateNoteListItemDto
    {
        [Required]
        public int NoteId { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public bool IsCompleted { get; set; } = false;
        public int Order { get; set; } = 0;
    }

    public class UpdateNoteListItemDto
    {
        [Required]
        public string Text { get; set; } = string.Empty;

        public bool IsCompleted { get; set; }
        public int Order { get; set; }
    }

    public class BulkOperationDto
    {
        [Required]
        public List<int> NoteIds { get; set; } = new List<int>();

        [Required]
        public string Operation { get; set; } = string.Empty; // archive, unarchive, trash, restore, delete, pin, unpin

        public string? Color { get; set; }
        public List<int>? LabelIds { get; set; }
    }

    public class NoteTemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public int UserId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateNoteTemplateDto
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

        public string? Color { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        public bool IsPublic { get; set; } = false;
    }

    public class NoteHistoryDto
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? PreviousTitle { get; set; }
        public string? PreviousContent { get; set; }
        public string? NewTitle { get; set; }
        public string? NewContent { get; set; }
        public string? PreviousColor { get; set; }
        public string? NewColor { get; set; }
        public string? ChangeDetails { get; set; }
        public DateTime ActionDateTime { get; set; }
    }

    public class AttachmentDownloadDto
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
    }
}
