using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Collaborators.Queries.GetNoteCollaborators
{
    public class GetNoteCollaboratorsQuery : IRequest<List<CollaboratorDto>>
    {
        public int NoteId { get; set; }
        public int UserId { get; set; }
    }
}
