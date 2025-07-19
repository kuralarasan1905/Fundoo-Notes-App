using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Notes.Commands.BulkRemoveLabel
{
    /// <summary>
    /// Handler for removing a label from multiple notes
    /// </summary>
    public class BulkRemoveLabelCommandHandler : IRequestHandler<BulkRemoveLabelCommand, BulkRemoveLabelResult>
    {
        private readonly INoteRepository _noteRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly ILogger<BulkRemoveLabelCommandHandler> _logger;

        public BulkRemoveLabelCommandHandler(
            INoteRepository noteRepository,
            ILabelRepository labelRepository,
            ILogger<BulkRemoveLabelCommandHandler> logger)
        {
            _noteRepository = noteRepository;
            _labelRepository = labelRepository;
            _logger = logger;
        }

        public async Task<BulkRemoveLabelResult> Handle(BulkRemoveLabelCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Bulk removing label {LabelId} from {NoteCount} notes for user: {UserId}", 
                request.LabelId, request.NoteIds.Count, request.UserId);

            var result = new BulkRemoveLabelResult();

            // Verify label exists and belongs to user
            var label = await _labelRepository.GetByIdAsync(request.LabelId, cancellationToken);
            if (label == null || label.UserId != request.UserId)
            {
                throw new KeyNotFoundException($"Label with ID {request.LabelId} not found");
            }

            // Process each note
            foreach (var noteId in request.NoteIds)
            {
                try
                {
                    // Verify note exists and belongs to user
                    var note = await _noteRepository.GetByIdAsync(noteId, cancellationToken);
                    if (note == null || note.UserId != request.UserId)
                    {
                        result.Errors.Add($"Note with ID {noteId} not found");
                        result.FailureCount++;
                        continue;
                    }

                    // Check if the label is associated with the note
                    var existingAssociation = note.NoteLabels.Any(nl => nl.LabelId == request.LabelId);
                    if (!existingAssociation)
                    {
                        _logger.LogDebug("Label {LabelId} is not associated with note {NoteId}", 
                            request.LabelId, noteId);
                        result.ProcessedNoteIds.Add(noteId);
                        result.SuccessCount++;
                        continue;
                    }

                    // Remove the label from the note
                    await _labelRepository.RemoveLabelFromNoteAsync(noteId, request.LabelId, cancellationToken);
                    
                    result.ProcessedNoteIds.Add(noteId);
                    result.SuccessCount++;

                    _logger.LogDebug("Successfully removed label {LabelId} from note {NoteId}", 
                        request.LabelId, noteId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error removing label {LabelId} from note {NoteId}", 
                        request.LabelId, noteId);
                    result.Errors.Add($"Failed to remove label from note {noteId}: {ex.Message}");
                    result.FailureCount++;
                }
            }

            _logger.LogInformation("Bulk remove label operation completed. Success: {SuccessCount}, Failures: {FailureCount}", 
                result.SuccessCount, result.FailureCount);

            return result;
        }
    }
}
