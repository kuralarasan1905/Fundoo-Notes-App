using MediatR;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Collaborators.Commands.RemoveCollaborator
{
    public class RemoveCollaboratorCommandHandler : IRequestHandler<RemoveCollaboratorCommand, bool>
    {
        private readonly ICollaboratorRepository _collaboratorRepository;
        private readonly INoteRepository _noteRepository;

        public RemoveCollaboratorCommandHandler(
            ICollaboratorRepository collaboratorRepository,
            INoteRepository noteRepository)
        {
            _collaboratorRepository = collaboratorRepository;
            _noteRepository = noteRepository;
        }

        public async Task<bool> Handle(RemoveCollaboratorCommand request, CancellationToken cancellationToken)
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

            await _collaboratorRepository.DeleteAsync(collaborator.Id);
            await _collaboratorRepository.SaveChangesAsync();

            return true;
        }
    }
}
