using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Notes.Commands.BulkRestoreNotes
{
    /// <summary>
    /// Handler for BulkRestoreNotesCommand
    /// </summary>
    public class BulkRestoreNotesCommandHandler : IRequestHandler<BulkRestoreNotesCommand, BulkRestoreNotesResult>
    {
        private readonly INoteRepository _noteRepository;
        private readonly ILogger<BulkRestoreNotesCommandHandler> _logger;

        public BulkRestoreNotesCommandHandler(
            INoteRepository noteRepository,
            ILogger<BulkRestoreNotesCommandHandler> logger)
        {
            _noteRepository = noteRepository;
            _logger = logger;
        }

        public async Task<BulkRestoreNotesResult> Handle(BulkRestoreNotesCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Bulk restoring {Count} notes from trash for user: {UserId}", request.NoteIds.Count, request.UserId);

            var result = new BulkRestoreNotesResult();

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

                    // Check if note is in trash
                    if (!note.IsTrashed)
                    {
                        result.Errors.Add($"Note with ID {noteId} is not in trash");
                        result.FailureCount++;
                        continue;
                    }

                    // Restore the note
                    await _noteRepository.TrashNoteAsync(noteId, false, cancellationToken);
                    
                    result.ProcessedNoteIds.Add(noteId);
                    result.SuccessCount++;

                    _logger.LogDebug("Successfully restored note {NoteId} for user: {UserId}", noteId, request.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to restore note {NoteId} for user: {UserId}", noteId, request.UserId);
                    result.Errors.Add($"Failed to restore note {noteId}: {ex.Message}");
                    result.FailureCount++;
                }
            }

            _logger.LogInformation("Bulk restore completed for user: {UserId}. Success: {SuccessCount}, Failures: {FailureCount}", 
                request.UserId, result.SuccessCount, result.FailureCount);

            return result;
        }
    }
}
