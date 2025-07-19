using MediatR;

namespace fundoo_notes.Application.Features.Notes.Commands.MoveNoteToTrash
{
    /// <summary>
    /// Command for moving a note to trash (soft delete)
    /// </summary>
    public class MoveNoteToTrashCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Set by the controller from JWT token
    }
}
