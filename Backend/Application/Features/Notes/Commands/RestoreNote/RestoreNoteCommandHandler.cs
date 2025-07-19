using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Notes.Commands.RestoreNote
{
    /// <summary>
    /// Handler for RestoreNoteCommand
    /// </summary>
    public class RestoreNoteCommandHandler : IRequestHandler<RestoreNoteCommand, NoteDto>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RestoreNoteCommandHandler> _logger;

        public RestoreNoteCommandHandler(
            INoteRepository noteRepository,
            IMapper mapper,
            ILogger<RestoreNoteCommandHandler> logger)
        {
            _noteRepository = noteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<NoteDto> Handle(RestoreNoteCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Restoring note {NoteId} from trash for user: {UserId}", request.Id, request.UserId);

            // Get the existing note
            var note = await _noteRepository.GetByIdAsync(request.Id, cancellationToken);
            if (note == null)
            {
                _logger.LogWarning("Note {NoteId} not found for restoration", request.Id);
                throw new KeyNotFoundException($"Note with ID {request.Id} not found");
            }

            // Check if user owns the note
            if (note.UserId != request.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to restore note {NoteId} without permission", request.UserId, request.Id);
                throw new UnauthorizedAccessException("You don't have permission to restore this note");
            }

            // Check if note is actually in trash
            if (!note.IsTrashed)
            {
                _logger.LogWarning("User {UserId} attempted to restore note {NoteId} that is not in trash", request.UserId, request.Id);
                throw new InvalidOperationException("Note is not in trash and cannot be restored");
            }

            // Restore the note from trash
            await _noteRepository.TrashNoteAsync(request.Id, false, cancellationToken);

            // Get the updated note with related data
            var updatedNote = await _noteRepository.GetByIdAsync(request.Id,
                n => n.User,
                n => n.NoteLabels,
                n => n.Collaborators);

            _logger.LogInformation("Note {NoteId} restored successfully by user: {UserId}", request.Id, request.UserId);

            return _mapper.Map<NoteDto>(updatedNote);
        }
    }
}
