using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Application.Features.Notes.Commands.AddLabelToNote
{
    /// <summary>
    /// Handler for adding a label to a note
    /// </summary>
    public class AddLabelToNoteCommandHandler : IRequestHandler<AddLabelToNoteCommand, bool>
    {
        private readonly INoteRepository _noteRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly ILogger<AddLabelToNoteCommandHandler> _logger;

        public AddLabelToNoteCommandHandler(
            INoteRepository noteRepository,
            ILabelRepository labelRepository,
            ILogger<AddLabelToNoteCommandHandler> logger)
        {
            _noteRepository = noteRepository;
            _labelRepository = labelRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(AddLabelToNoteCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Adding label {LabelId} to note {NoteId} for user: {UserId}", 
                request.LabelId, request.NoteId, request.UserId);

            // Verify note exists and belongs to user
            var note = await _noteRepository.GetByIdAsync(request.NoteId, cancellationToken);
            if (note == null || note.UserId != request.UserId)
            {
                throw new KeyNotFoundException($"Note with ID {request.NoteId} not found");
            }

            // Verify label exists and belongs to user
            var label = await _labelRepository.GetByIdAsync(request.LabelId, cancellationToken);
            if (label == null || label.UserId != request.UserId)
            {
                throw new KeyNotFoundException($"Label with ID {request.LabelId} not found");
            }

            // Check if the label is already associated with the note
            var existingAssociation = note.NoteLabels.Any(nl => nl.LabelId == request.LabelId);
            if (existingAssociation)
            {
                _logger.LogWarning("Label {LabelId} is already associated with note {NoteId}", 
                    request.LabelId, request.NoteId);
                return false; // Already exists, no action needed
            }

            // Add the label to the note
            await _labelRepository.AddLabelToNoteAsync(request.NoteId, request.LabelId, cancellationToken);

            _logger.LogInformation("Successfully added label {LabelId} to note {NoteId} for user: {UserId}", 
                request.LabelId, request.NoteId, request.UserId);

            return true;
        }
    }
}
