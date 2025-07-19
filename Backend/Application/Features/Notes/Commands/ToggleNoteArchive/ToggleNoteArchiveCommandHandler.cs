using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Notes.Commands.ToggleNoteArchive
{
    /// <summary>
    /// Handler for ToggleNoteArchiveCommand
    /// </summary>
    public class ToggleNoteArchiveCommandHandler : IRequestHandler<ToggleNoteArchiveCommand, NoteDto>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ToggleNoteArchiveCommandHandler> _logger;

        public ToggleNoteArchiveCommandHandler(
            INoteRepository noteRepository,
            IMapper mapper,
            ILogger<ToggleNoteArchiveCommandHandler> logger)
        {
            _noteRepository = noteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<NoteDto> Handle(ToggleNoteArchiveCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Toggling archive status for note {NoteId} by user: {UserId}", request.Id, request.UserId);

            // Get the existing note
            var note = await _noteRepository.GetByIdAsync(request.Id, cancellationToken);
            if (note == null)
            {
                throw new KeyNotFoundException($"Note with ID {request.Id} not found");
            }

            // Check if user owns the note
            if (note.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("You don't have permission to modify this note");
            }

            // Toggle archive status
            note.IsArchived = !note.IsArchived;

            // Update the note
            await _noteRepository.UpdateAsync(note, cancellationToken);
            await _noteRepository.SaveChangesAsync(cancellationToken);

            // Retrieve the updated note with related data
            var updatedNote = await _noteRepository.GetByIdAsync(note.Id, 
                n => n.User, 
                n => n.NoteLabels, 
                n => n.Collaborators);

            _logger.LogInformation("Note {NoteId} archive status toggled to {IsArchived}", note.Id, note.IsArchived);

            return _mapper.Map<NoteDto>(updatedNote);
        }
    }
}
