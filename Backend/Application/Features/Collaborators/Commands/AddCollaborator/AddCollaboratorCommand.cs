using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Collaborators.Commands.AddCollaborator
{
    public class AddCollaboratorCommand : IRequest<CollaboratorDto>
    {
        public int NoteId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string Permission { get; set; } = "Read";
        public int RequesterId { get; set; }
    }
}
