using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Notes.Commands.RemoveLabelFromNote
{
    /// <summary>
    /// Handler for removing a label from a note
    /// </summary>
    public class RemoveLabelFromNoteCommandHandler : IRequestHandler<RemoveLabelFromNoteCommand, bool>
    {
        private readonly INoteRepository _noteRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly ILogger<RemoveLabelFromNoteCommandHandler> _logger;

        public RemoveLabelFromNoteCommandHandler(
            INoteRepository noteRepository,
            ILabelRepository labelRepository,
            ILogger<RemoveLabelFromNoteCommandHandler> logger)
        {
            _noteRepository = noteRepository;
            _labelRepository = labelRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(RemoveLabelFromNoteCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Removing label {LabelId} from note {NoteId} for user: {UserId}", 
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

            // Check if the label is associated with the note
            var existingAssociation = note.NoteLabels.Any(nl => nl.LabelId == request.LabelId);
            if (!existingAssociation)
            {
                _logger.LogWarning("Label {LabelId} is not associated with note {NoteId}", 
                    request.LabelId, request.NoteId);
                return false; // Not associated, no action needed
            }

            // Remove the label from the note
            await _labelRepository.RemoveLabelFromNoteAsync(request.NoteId, request.LabelId, cancellationToken);

            _logger.LogInformation("Successfully removed label {LabelId} from note {NoteId} for user: {UserId}", 
                request.LabelId, request.NoteId, request.UserId);

            return true;
        }
    }
}
