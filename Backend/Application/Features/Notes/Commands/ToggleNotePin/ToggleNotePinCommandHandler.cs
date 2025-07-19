using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Notes.Commands.ToggleNotePin
{
    /// <summary>
    /// Handler for ToggleNotePinCommand
    /// </summary>
    public class ToggleNotePinCommandHandler : IRequestHandler<ToggleNotePinCommand, NoteDto>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ToggleNotePinCommandHandler> _logger;

        public ToggleNotePinCommandHandler(
            INoteRepository noteRepository,
            IMapper mapper,
            ILogger<ToggleNotePinCommandHandler> logger)
        {
            _noteRepository = noteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<NoteDto> Handle(ToggleNotePinCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Toggling pin status for note {NoteId} by user: {UserId}", request.Id, request.UserId);

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

            // Toggle pin status
            note.IsPinned = !note.IsPinned;

            // Update the note
            await _noteRepository.UpdateAsync(note, cancellationToken);
            await _noteRepository.SaveChangesAsync(cancellationToken);

            // Retrieve the updated note with related data
            var updatedNote = await _noteRepository.GetByIdAsync(note.Id, 
                n => n.User, 
                n => n.NoteLabels, 
                n => n.Collaborators);

            _logger.LogInformation("Note {NoteId} pin status toggled to {IsPinned}", note.Id, note.IsPinned);

            return _mapper.Map<NoteDto>(updatedNote);
        }
    }
}
