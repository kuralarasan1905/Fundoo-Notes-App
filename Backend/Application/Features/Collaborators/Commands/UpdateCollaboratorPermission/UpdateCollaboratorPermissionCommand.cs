using MediatR;

namespace fundoo_notes.Application.Features.Collaborators.Commands.UpdateCollaboratorPermission
{
    public class UpdateCollaboratorPermissionCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string Permission { get; set; } = string.Empty;
        public int RequesterId { get; set; }
    }
}
