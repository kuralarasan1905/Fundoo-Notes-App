using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Notes.Commands.BulkRestoreNotes
{
    /// <summary>
    /// Command for bulk restoring notes from trash
    /// </summary>
    public class BulkRestoreNotesCommand : IRequest<BulkRestoreNotesResult>
    {
        public List<int> NoteIds { get; set; } = new List<int>();
        public int UserId { get; set; } // Set by the controller from JWT token
    }
}
