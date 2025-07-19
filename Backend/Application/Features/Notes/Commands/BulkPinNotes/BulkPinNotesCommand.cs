using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Notes.Commands.BulkPinNotes
{
    /// <summary>
    /// Command for bulk pin/unpin notes operation
    /// </summary>
    public class BulkPinNotesCommand : IRequest<BulkPinNotesResult>
    {
        public List<int> NoteIds { get; set; } = new List<int>();
        public bool IsPinned { get; set; }
        public int UserId { get; set; } // Set by the controller from JWT token
    }

    /// <summary>
    /// Result of bulk pin/unpin operation
    /// </summary>
    public class BulkPinNotesResult
    {
        public List<NoteDto> UpdatedNotes { get; set; } = new List<NoteDto>();
        public List<int> FailedNoteIds { get; set; } = new List<int>();
        public int SuccessCount => UpdatedNotes.Count;
        public int FailedCount => FailedNoteIds.Count;
        public string Message => $"Pin status updated for {SuccessCount} out of {SuccessCount + FailedCount} notes";
    }
}
