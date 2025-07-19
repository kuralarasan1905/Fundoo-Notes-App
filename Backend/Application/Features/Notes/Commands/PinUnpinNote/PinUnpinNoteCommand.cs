using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Notes.Commands.PinUnpinNote
{
    /// <summary>
    /// Command for single note pin/unpin operation (matches frontend request)
    /// </summary>
    public class PinUnpinNoteCommand : IRequest<NoteDto>
    {
        public int NoteId { get; set; }
        public bool IsPinned { get; set; }
        public int UserId { get; set; } // Set by the controller from JWT token or user context
    }
}
