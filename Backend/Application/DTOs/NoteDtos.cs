using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using fundoo_notes.Application.Common.Helpers;

namespace fundoo_notes.Application.DTOs
{
    /// <summary>
    /// Data Transfer Objects for Note operations
    /// </summary>
    
    public class CreateNoteDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [StringLength(5000)]
        public string Content { get; set; } = string.Empty;

        [StringLength(50)] // Allow for pattern names like "grid", "lines", "dots", "gradient"
        public string? Color { get; set; }

        public DateTime? ReminderDateTime { get; set; }

        public List<int> LabelIds { get; set; } = new List<int>();
    }

    public class UpdateNoteDto
    {
        public int Id { get; set; } // Added Id property for bulk updates

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [StringLength(5000)]
        public string Content { get; set; } = string.Empty;

        [StringLength(50)] // Allow for pattern names
        public string? Color { get; set; }

        public DateTime? ReminderDateTime { get; set; }

        public List<int>? LabelIds { get; set; } = null;

        // Alternative property names for frontend compatibility
        [StringLength(5000)]
        public string? Description
        {
            get => Content;
            set => Content = value ?? string.Empty;
        }
    }

    public class NoteDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string PreviewContent { get; set; } = string.Empty;
        public string? Color { get; set; }

        // Computed properties for frontend
        public string DisplayColor => ColorHelper.GetDisplayColor(Color);
        public string? PatternType => ColorHelper.GetPatternType(Color);
        public bool IsPattern => ColorHelper.IsPattern(Color);

        public bool IsPinned { get; set; }
        public bool IsArchived { get; set; }
        public bool IsTrashed { get; set; }
        public DateTime? ReminderDateTime { get; set; }
        public bool HasReminder { get; set; }
        public bool IsOverdue { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<LabelDto> Labels { get; set; } = new List<LabelDto>();
        public List<int> LabelIds { get; set; } = new List<int>();
        public List<CollaboratorDto> Collaborators { get; set; } = new List<CollaboratorDto>();
        public List<NoteAttachmentDto> Attachments { get; set; } = new List<NoteAttachmentDto>();
        public List<NoteReminderDto> Reminders { get; set; } = new List<NoteReminderDto>();
        public List<NoteListItemDto> ListItems { get; set; } = new List<NoteListItemDto>();
        public bool IsListNote { get; set; }
        public int CompletedItemsCount { get; set; }
        public int TotalItemsCount { get; set; }
    }

    public class NoteListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string PreviewContent { get; set; } = string.Empty;
        public string? Color { get; set; }
        public bool IsPinned { get; set; }
        public bool IsArchived { get; set; }
        public bool IsTrashed { get; set; }
        public DateTime? ReminderDateTime { get; set; }
        public bool HasReminder { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<LabelDto> Labels { get; set; } = new List<LabelDto>();
        public int CollaboratorCount { get; set; }
        public int AttachmentCount { get; set; }
        public bool IsListNote { get; set; }
        public int CompletedItemsCount { get; set; }
        public int TotalItemsCount { get; set; }
    }

    public class PinNoteDto
    {
        public bool IsPinned { get; set; }
    }

    public class ArchiveNoteDto
    {
        public bool IsArchived { get; set; }
    }

    public class TrashNoteDto
    {
        public bool IsTrashed { get; set; }
    }

    public class NoteSearchDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class NotesPagedResultDto
    {
        public List<NoteListDto> Notes { get; set; } = new List<NoteListDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    // Bulk operation DTOs
    public class BulkArchiveDto
    {
        [Required]
        public List<string> NoteIdList { get; set; } = new List<string>();

        [Required]
        public bool IsArchived { get; set; }

        // Helper property to convert string IDs to integers
        public List<int> GetNoteIds()
        {
            return NoteIdList.Where(id => int.TryParse(id, out _))
                           .Select(int.Parse)
                           .ToList();
        }
    }

    public class BulkTrashDto
    {
        [Required]
        public List<string> NoteIdList { get; set; } = new List<string>();

        [Required]
        public bool IsDeleted { get; set; }

        // Helper property to convert string IDs to integers
        public List<int> GetNoteIds()
        {
            return NoteIdList.Where(id => int.TryParse(id, out _))
                           .Select(int.Parse)
                           .ToList();
        }
    }

    public class BulkDeleteDto
    {
        [Required]
        public List<string> NoteIdList { get; set; } = new List<string>();

        [Required]
        public bool IsDeleted { get; set; }

        // Helper property to convert string IDs to integers
        public List<int> GetNoteIds()
        {
            return NoteIdList.Where(id => int.TryParse(id, out _))
                           .Select(int.Parse)
                           .ToList();
        }
    }

    /// <summary>
    /// DTO for single note pin/unpin operation (matches frontend request)
    /// </summary>
    public class PinUnpinNoteRequest
    {
        [Required(ErrorMessage = "Note ID is required")]
        public int NoteId { get; set; }

        public bool IsPinned { get; set; }
    }

    /// <summary>
    /// DTO for bulk pin/unpin operations (for future use)
    /// </summary>
    public class BulkPinDto
    {
        [Required(ErrorMessage = "Note IDs list is required")]
        [MinLength(1, ErrorMessage = "At least one note ID must be provided")]
        public List<string> NoteIdList { get; set; } = new List<string>();

        public bool IsPinned { get; set; }

        // Helper property to convert string IDs to integers
        public List<int> GetNoteIds()
        {
            var validIds = NoteIdList?.Where(id => !string.IsNullOrWhiteSpace(id) && int.TryParse(id, out _))
                                    .Select(int.Parse)
                                    .ToList() ?? new List<int>();

            return validIds;
        }

        // Validation method to check if we have valid note IDs
        public bool HasValidNoteIds()
        {
            return GetNoteIds().Any();
        }
    }

    public class BulkColorChangeDto
    {
        [Required]
        public List<string> NoteIdList { get; set; } = new List<string>();

        [StringLength(50)] // Updated to support pattern names
        public string? Color { get; set; }

        // Helper property to convert string IDs to integers
        public List<int> GetNoteIds()
        {
            return NoteIdList.Where(id => int.TryParse(id, out _))
                           .Select(int.Parse)
                           .ToList();
        }
    }

    public class BulkReminderDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one note ID is required")]
        [JsonPropertyName("noteIdList")]
        public List<string> NoteIdList { get; set; } = new List<string>();

        [Required(ErrorMessage = "Reminder date and time is required")]
        [JsonPropertyName("reminderDateTime")]
        public DateTime? ReminderDateTime { get; set; }

        // Helper property to convert string IDs to integers
        public List<int> GetNoteIds()
        {
            return NoteIdList.Where(id => int.TryParse(id, out _))
                           .Select(int.Parse)
                           .ToList();
        }

        // Validation method
        public bool IsValid(out List<string> errors)
        {
            errors = new List<string>();

            if (NoteIdList == null || !NoteIdList.Any())
            {
                errors.Add("Note IDs list cannot be empty");
            }

            if (!ReminderDateTime.HasValue)
            {
                errors.Add("Reminder date and time is required");
            }
            else
            {
                var currentUtc = DateTime.UtcNow;
                var reminderUtc = ReminderDateTime.Value.Kind == DateTimeKind.Utc
                    ? ReminderDateTime.Value
                    : ReminderDateTime.Value.ToUniversalTime();

                if (reminderUtc <= currentUtc)
                {
                    errors.Add($"Reminder date and time must be in the future. Received: {reminderUtc:yyyy-MM-ddTHH:mm:ss.fffZ}, Current UTC: {currentUtc:yyyy-MM-ddTHH:mm:ss.fffZ}");
                }
            }

            var invalidIds = NoteIdList?.Where(id => !int.TryParse(id, out _)).ToList() ?? new List<string>();
            if (invalidIds.Any())
            {
                errors.Add($"Invalid note IDs: {string.Join(", ", invalidIds)}");
            }

            return !errors.Any();
        }
    }

    public class BulkRemoveReminderDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one note ID is required")]
        [JsonPropertyName("noteIdList")]
        public List<string> NoteIdList { get; set; } = new List<string>();

        // Helper property to convert string IDs to integers
        public List<int> GetNoteIds()
        {
            return NoteIdList.Where(id => int.TryParse(id, out _))
                           .Select(int.Parse)
                           .ToList();
        }

        // Validation method
        public bool IsValid(out List<string> errors)
        {
            errors = new List<string>();

            if (NoteIdList == null || !NoteIdList.Any())
            {
                errors.Add("Note IDs list cannot be empty");
            }

            var invalidIds = NoteIdList.Where(id => !int.TryParse(id, out _)).ToList();
            if (invalidIds.Any())
            {
                errors.Add($"Invalid note IDs: {string.Join(", ", invalidIds)}");
            }

            return !errors.Any();
        }
    }

    public class BulkUpdateNotesDto
    {
        [Required]
        public List<UpdateNoteDto> Notes { get; set; } = new List<UpdateNoteDto>();
    }

    // Label management DTOs
    public class AddLabelToNoteDto
    {
        [Required]
        public int NoteId { get; set; }

        [Required]
        public int LabelId { get; set; }
    }

    public class RemoveLabelFromNoteDto
    {
        [Required]
        public int NoteId { get; set; }

        [Required]
        public int LabelId { get; set; }
    }

    public class BulkAddLabelDto
    {
        [Required]
        public List<int> NoteIds { get; set; } = new List<int>();

        [Required]
        public int LabelId { get; set; }
    }

    public class BulkRemoveLabelDto
    {
        [Required]
        public List<int> NoteIds { get; set; } = new List<int>();

        [Required]
        public int LabelId { get; set; }
    }

    public class ManageNoteLabelsDto
    {
        [Required]
        public int NoteId { get; set; }

        [Required]
        public List<int> LabelIds { get; set; } = new List<int>();
    }

    // Restore operation DTOs
    public class BulkRestoreNotesResult
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<int> ProcessedNoteIds { get; set; } = new List<int>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}
