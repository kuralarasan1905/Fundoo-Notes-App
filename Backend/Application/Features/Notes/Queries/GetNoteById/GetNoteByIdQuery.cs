using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Notes.Queries.GetNoteById
{
    /// <summary>
    /// Query for getting a specific note by ID
    /// </summary>
    public class GetNoteByIdQuery : IRequest<NoteDto?>
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Set by the controller from JWT token
    }
}
