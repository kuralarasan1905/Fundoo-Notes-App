using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Notes.Commands.BulkAddLabel
{
    /// <summary>
    /// Handler for adding a label to multiple notes
    /// </summary>
    public class BulkAddLabelCommandHandler : IRequestHandler<BulkAddLabelCommand, BulkAddLabelResult>
    {
        private readonly INoteRepository _noteRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly ILogger<BulkAddLabelCommandHandler> _logger;

        public BulkAddLabelCommandHandler(
            INoteRepository noteRepository,
            ILabelRepository labelRepository,
            ILogger<BulkAddLabelCommandHandler> logger)
        {
            _noteRepository = noteRepository;
            _labelRepository = labelRepository;
            _logger = logger;
        }

        public async Task<BulkAddLabelResult> Handle(BulkAddLabelCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Bulk adding label {LabelId} to {NoteCount} notes for user: {UserId}", 
                request.LabelId, request.NoteIds.Count, request.UserId);

            var result = new BulkAddLabelResult();

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

                    // Check if the label is already associated with the note
                    var existingAssociation = note.NoteLabels.Any(nl => nl.LabelId == request.LabelId);
                    if (existingAssociation)
                    {
                        _logger.LogDebug("Label {LabelId} is already associated with note {NoteId}", 
                            request.LabelId, noteId);
                        result.ProcessedNoteIds.Add(noteId);
                        result.SuccessCount++;
                        continue;
                    }

                    // Add the label to the note
                    await _labelRepository.AddLabelToNoteAsync(noteId, request.LabelId, cancellationToken);
                    
                    result.ProcessedNoteIds.Add(noteId);
                    result.SuccessCount++;

                    _logger.LogDebug("Successfully added label {LabelId} to note {NoteId}", 
                        request.LabelId, noteId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding label {LabelId} to note {NoteId}", 
                        request.LabelId, noteId);
                    result.Errors.Add($"Failed to add label to note {noteId}: {ex.Message}");
                    result.FailureCount++;
                }
            }

            _logger.LogInformation("Bulk add label operation completed. Success: {SuccessCount}, Failures: {FailureCount}", 
                result.SuccessCount, result.FailureCount);

            return result;
        }
    }
}
