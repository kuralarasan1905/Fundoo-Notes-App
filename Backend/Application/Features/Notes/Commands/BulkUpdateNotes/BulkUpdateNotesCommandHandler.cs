using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Application.Common.Helpers;

namespace fundoo_notes.Application.Features.Notes.Commands.BulkUpdateNotes
{
    /// <summary>
    /// Optimized bulk update handler that processes multiple notes in a single transaction
    /// </summary>
    public class BulkUpdateNotesCommandHandler : IRequestHandler<BulkUpdateNotesCommand, BulkUpdateNotesResponse>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BulkUpdateNotesCommandHandler> _logger;

        public BulkUpdateNotesCommandHandler(
            INoteRepository noteRepository,
            IMapper mapper,
            ILogger<BulkUpdateNotesCommandHandler> logger)
        {
            _noteRepository = noteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BulkUpdateNotesResponse> Handle(BulkUpdateNotesCommand request, CancellationToken cancellationToken)
        {
            var response = new BulkUpdateNotesResponse();
            
            if (!request.Notes.Any())
            {
                _logger.LogWarning("Bulk update requested with no notes for user: {UserId}", request.UserId);
                return response;
            }

            _logger.LogInformation("Starting bulk update of {Count} notes for user: {UserId}", 
                request.Notes.Count, request.UserId);

            // Start a transaction for all updates
            using var transaction = await _noteRepository.BeginTransactionAsync(cancellationToken);
            
            try
            {
                // Get all notes to update in a single query
                var noteIds = request.Notes.Select(n => n.Id).ToList();
                var existingNotes = await _noteRepository.GetNotesByIdsAsync(noteIds, request.UserId, cancellationToken);
                var existingNotesDict = existingNotes.ToDictionary(n => n.Id);

                var updatedNotes = new List<Domain.Entities.Note>();

                // Process each update request
                foreach (var updateRequest in request.Notes)
                {
                    try
                    {
                        if (!existingNotesDict.TryGetValue(updateRequest.Id, out var note))
                        {
                            response.Errors.Add(new BulkUpdateError
                            {
                                NoteId = updateRequest.Id,
                                Error = "Note not found",
                                Details = $"Note with ID {updateRequest.Id} not found or access denied"
                            });
                            continue;
                        }

                        // Apply updates only for non-null values
                        var hasChanges = false;

                        if (!string.IsNullOrEmpty(updateRequest.Title) && note.Title != updateRequest.Title)
                        {
                            note.Title = updateRequest.Title;
                            hasChanges = true;
                        }

                        if (!string.IsNullOrEmpty(updateRequest.Content) && note.Content != updateRequest.Content)
                        {
                            note.Content = updateRequest.Content;
                            hasChanges = true;
                        }

                        if (!string.IsNullOrEmpty(updateRequest.Color))
                        {
                            var normalizedColor = ColorHelper.NormalizeColor(updateRequest.Color);
                            if (ColorHelper.IsValidColor(normalizedColor) && note.Color != normalizedColor)
                            {
                                note.Color = normalizedColor;
                                hasChanges = true;
                            }
                        }

                        if (updateRequest.ReminderDateTime.HasValue && note.ReminderDateTime != updateRequest.ReminderDateTime)
                        {
                            note.ReminderDateTime = updateRequest.ReminderDateTime;
                            hasChanges = true;
                        }

                        if (updateRequest.IsPinned.HasValue && note.IsPinned != updateRequest.IsPinned.Value)
                        {
                            note.IsPinned = updateRequest.IsPinned.Value;
                            hasChanges = true;
                        }

                        if (updateRequest.IsArchived.HasValue && note.IsArchived != updateRequest.IsArchived.Value)
                        {
                            note.IsArchived = updateRequest.IsArchived.Value;
                            hasChanges = true;
                        }

                        if (updateRequest.IsTrashed.HasValue && note.IsTrashed != updateRequest.IsTrashed.Value)
                        {
                            note.IsTrashed = updateRequest.IsTrashed.Value;
                            hasChanges = true;
                        }

                        if (hasChanges)
                        {
                            note.UpdatedAt = DateTime.UtcNow;
                            updatedNotes.Add(note);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating note {NoteId} in bulk operation", updateRequest.Id);
                        response.Errors.Add(new BulkUpdateError
                        {
                            NoteId = updateRequest.Id,
                            Error = "Update failed",
                            Details = ex.Message
                        });
                    }
                }

                // Bulk update all changed notes
                if (updatedNotes.Any())
                {
                    await _noteRepository.BulkUpdateAsync(updatedNotes, cancellationToken);
                    await _noteRepository.SaveChangesAsync(cancellationToken);

                    // Map to DTOs for response
                    response.UpdatedNotes = _mapper.Map<List<NoteDto>>(updatedNotes);
                }

                await transaction.CommitAsync(cancellationToken);

                response.SuccessCount = updatedNotes.Count;
                response.ErrorCount = response.Errors.Count;

                _logger.LogInformation("Bulk update completed: {SuccessCount} updated, {ErrorCount} errors for user: {UserId}", 
                    response.SuccessCount, response.ErrorCount, request.UserId);

                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Bulk update transaction failed for user: {UserId}", request.UserId);
                
                response.Errors.Add(new BulkUpdateError
                {
                    NoteId = 0,
                    Error = "Transaction failed",
                    Details = "The bulk update operation failed and was rolled back"
                });
                
                return response;
            }
        }
    }
}
