using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Notes.Commands.PinUnpinNote
{
    /// <summary>
    /// Handler for single note pin/unpin operation
    /// </summary>
    public class PinUnpinNoteCommandHandler : IRequestHandler<PinUnpinNoteCommand, NoteDto>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PinUnpinNoteCommandHandler> _logger;

        public PinUnpinNoteCommandHandler(
            INoteRepository noteRepository,
            IMapper mapper,
            ILogger<PinUnpinNoteCommandHandler> logger)
        {
            _noteRepository = noteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<NoteDto> Handle(PinUnpinNoteCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Setting pin status for note {NoteId} to {IsPinned} by user: {UserId}", 
                request.NoteId, request.IsPinned, request.UserId);

            // Get the existing note
            var note = await _noteRepository.GetByIdAsync(request.NoteId, cancellationToken);
            if (note == null)
            {
                throw new KeyNotFoundException($"Note with ID {request.NoteId} not found");
            }

            // Check if user owns the note
            if (note.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("You don't have permission to modify this note");
            }

            // Set pin status (not toggle, but set to specific value)
            note.IsPinned = request.IsPinned;
            note.UpdatedAt = DateTime.UtcNow;

            // Update the note
            await _noteRepository.UpdateAsync(note, cancellationToken);
            await _noteRepository.SaveChangesAsync(cancellationToken);

            // Retrieve the updated note with related data
            var updatedNote = await _noteRepository.GetByIdAsync(note.Id, 
                n => n.User, 
                n => n.NoteLabels, 
                n => n.Collaborators);

            _logger.LogInformation("Note {NoteId} pin status set to {IsPinned}", note.Id, note.IsPinned);

            return _mapper.Map<NoteDto>(updatedNote);
        }
    }
}
