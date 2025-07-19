using MediatR;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Collaborators.Commands.UpdateCollaboratorPermission
{
    public class UpdateCollaboratorPermissionCommandHandler : IRequestHandler<UpdateCollaboratorPermissionCommand, bool>
    {
        private readonly ICollaboratorRepository _collaboratorRepository;
        private readonly INoteRepository _noteRepository;

        public UpdateCollaboratorPermissionCommandHandler(
            ICollaboratorRepository collaboratorRepository,
            INoteRepository noteRepository)
        {
            _collaboratorRepository = collaboratorRepository;
            _noteRepository = noteRepository;
        }

        public async Task<bool> Handle(UpdateCollaboratorPermissionCommand request, CancellationToken cancellationToken)
        {
            var collaborator = await _collaboratorRepository.GetByIdAsync(request.Id);
            if (collaborator == null)
            {
                return false;
            }

            // Verify user owns the note
            var note = await _noteRepository.GetByIdAsync(collaborator.NoteId);
            if (note == null || note.UserId != request.RequesterId)
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            collaborator.Permission = request.Permission;
            await _collaboratorRepository.UpdateAsync(collaborator);
            await _collaboratorRepository.SaveChangesAsync();

            return true;
        }
    }
}
