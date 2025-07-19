using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Notes.Commands.ToggleNoteArchive
{
    /// <summary>
    /// Command for toggling note archive status
    /// </summary>
    public class ToggleNoteArchiveCommand : IRequest<NoteDto>
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Set by the controller from JWT token
    }
}
