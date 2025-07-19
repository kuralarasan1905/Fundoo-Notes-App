using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Notes.Commands.ToggleNoteTrash
{
    /// <summary>
    /// Command for toggling note trash status (move to/from trash)
    /// </summary>
    public class ToggleNoteTrashCommand : IRequest<NoteDto>
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Set by the controller from JWT token
    }
}
