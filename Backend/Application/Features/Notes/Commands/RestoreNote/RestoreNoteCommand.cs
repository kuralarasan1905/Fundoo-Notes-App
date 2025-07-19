using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Notes.Commands.RestoreNote
{
    /// <summary>
    /// Command for restoring a note from trash
    /// </summary>
    public class RestoreNoteCommand : IRequest<NoteDto>
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Set by the controller from JWT token
    }
}
