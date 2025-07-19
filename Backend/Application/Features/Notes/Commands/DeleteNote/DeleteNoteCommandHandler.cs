using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Notes.Commands.DeleteNote
{
    /// <summary>
    /// Handler for DeleteNoteCommand
    /// </summary>
    public class DeleteNoteCommandHandler : IRequestHandler<DeleteNoteCommand, bool>
    {
        private readonly INoteRepository _noteRepository;
        private readonly ILogger<DeleteNoteCommandHandler> _logger;

        public DeleteNoteCommandHandler(
            INoteRepository noteRepository,
            ILogger<DeleteNoteCommandHandler> logger)
        {
            _noteRepository = noteRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Permanently deleting note {NoteId} for user: {UserId}", request.Id, request.UserId);

            // Get the existing note
            var note = await _noteRepository.GetByIdAsync(request.Id, cancellationToken);
            if (note == null)
            {
                _logger.LogWarning("Note {NoteId} not found for deletion", request.Id);
                throw new KeyNotFoundException($"Note with ID {request.Id} not found");
            }

            // Check if user owns the note
            if (note.UserId != request.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to delete note {NoteId} without permission", request.UserId, request.Id);
                throw new UnauthorizedAccessException("You don't have permission to delete this note");
            }

            // For permanent deletion, only allow if note is already in trash
            if (!note.IsTrashed)
            {
                _logger.LogWarning("User {UserId} attempted to permanently delete note {NoteId} that is not in trash", request.UserId, request.Id);
                throw new InvalidOperationException("Note must be moved to trash before permanent deletion");
            }

            // Permanently delete the note (hard delete)
            await _noteRepository.DeleteAsync(request.Id, cancellationToken);
            await _noteRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Note {NoteId} permanently deleted by user: {UserId}", request.Id, request.UserId);

            return true;
        }
    }
}
