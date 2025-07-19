using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Notes.Commands.ToggleNoteTrash
{
    /// <summary>
    /// Handler for toggling note trash status (move to/from trash)
    /// </summary>
    public class ToggleNoteTrashCommandHandler : IRequestHandler<ToggleNoteTrashCommand, NoteDto>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ToggleNoteTrashCommandHandler> _logger;

        public ToggleNoteTrashCommandHandler(
            INoteRepository noteRepository,
            IMapper mapper,
            ILogger<ToggleNoteTrashCommandHandler> logger)
        {
            _noteRepository = noteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<NoteDto> Handle(ToggleNoteTrashCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Toggling trash status for note {NoteId} by user: {UserId}", request.Id, request.UserId);

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
                _logger.LogWarning("User {UserId} attempted to toggle trash status for note {NoteId} without permission", request.UserId, request.Id);
                throw new UnauthorizedAccessException("You don't have permission to modify this note");
            }

            // Toggle trash status
            var newTrashStatus = !note.IsTrashed;
            await _noteRepository.TrashNoteAsync(request.Id, newTrashStatus, cancellationToken);

            // Get the updated note
            var updatedNote = await _noteRepository.GetByIdAsync(request.Id, cancellationToken);
            var noteDto = _mapper.Map<NoteDto>(updatedNote);

            var action = newTrashStatus ? "moved to trash" : "restored from trash";
            _logger.LogInformation("Note {NoteId} successfully {Action} by user: {UserId}", request.Id, action, request.UserId);

            return noteDto;
        }
    }
}
