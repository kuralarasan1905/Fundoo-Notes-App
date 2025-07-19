using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Notes.Commands.MoveNoteToTrash
{
    /// <summary>
    /// Handler for moving a note to trash (soft delete)
    /// </summary>
    public class MoveNoteToTrashCommandHandler : IRequestHandler<MoveNoteToTrashCommand, bool>
    {
        private readonly INoteRepository _noteRepository;
        private readonly ILogger<MoveNoteToTrashCommandHandler> _logger;

        public MoveNoteToTrashCommandHandler(
            INoteRepository noteRepository,
            ILogger<MoveNoteToTrashCommandHandler> logger)
        {
            _noteRepository = noteRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(MoveNoteToTrashCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Moving note {NoteId} to trash for user: {UserId}", request.Id, request.UserId);

            // Verify the note exists and user has access
            var note = await _noteRepository.GetByIdAsync(request.Id, cancellationToken);
            if (note == null)
            {
                _logger.LogWarning("Note {NoteId} not found for user: {UserId}", request.Id, request.UserId);
                throw new KeyNotFoundException($"Note with ID {request.Id} not found");
            }

            // Verify user ownership
            if (note.UserId != request.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to move note {NoteId} to trash without permission", request.UserId, request.Id);
                throw new UnauthorizedAccessException("You don't have permission to move this note to trash");
            }

            // Check if note is already in trash
            if (note.IsTrashed)
            {
                _logger.LogInformation("Note {NoteId} is already in trash", request.Id);
                return true; // Already in trash, consider it successful
            }

            // Move to trash (soft delete)
            await _noteRepository.TrashNoteAsync(request.Id, true, cancellationToken);

            _logger.LogInformation("Note {NoteId} successfully moved to trash by user: {UserId}", request.Id, request.UserId);

            return true;
        }
    }
}
