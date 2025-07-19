using MediatR;

namespace fundoo_notes.Application.Features.Notes.Commands.DeleteNote
{
    /// <summary>
    /// Command for deleting a note (soft delete)
    /// </summary>
    public class DeleteNoteCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Set by the controller from JWT token
    }
}
