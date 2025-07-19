using MediatR;

namespace fundoo_notes.Application.Features.Collaborators.Commands.RemoveCollaborator
{
    public class RemoveCollaboratorCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public int RequesterId { get; set; }
    }
}
