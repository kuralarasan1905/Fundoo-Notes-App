using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Notes.Commands.ToggleNotePin
{
    /// <summary>
    /// Command for toggling note pin status
    /// </summary>
    public class ToggleNotePinCommand : IRequest<NoteDto>
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Set by the controller from JWT token
    }
}
