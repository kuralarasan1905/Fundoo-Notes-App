using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Notes.Commands.BulkUpdateNotes
{
    /// <summary>
    /// Command for bulk updating multiple notes in a single transaction
    /// </summary>
    public class BulkUpdateNotesCommand : IRequest<BulkUpdateNotesResponse>
    {
        public int UserId { get; set; }
        public List<BulkNoteUpdateDto> Notes { get; set; } = new List<BulkNoteUpdateDto>();
    }

    public class BulkNoteUpdateDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Color { get; set; }
        public DateTime? ReminderDateTime { get; set; }
        public List<int>? LabelIds { get; set; }
        public bool? IsPinned { get; set; }
        public bool? IsArchived { get; set; }
        public bool? IsTrashed { get; set; }
    }

    public class BulkUpdateNotesResponse
    {
        public List<NoteDto> UpdatedNotes { get; set; } = new List<NoteDto>();
        public List<BulkUpdateError> Errors { get; set; } = new List<BulkUpdateError>();
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
    }

    public class BulkUpdateError
    {
        public int NoteId { get; set; }
        public string Error { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}
