using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Notes.Commands.BulkPinNotes
{
    /// <summary>
    /// Handler for BulkPinNotesCommand
    /// </summary>
    public class BulkPinNotesCommandHandler : IRequestHandler<BulkPinNotesCommand, BulkPinNotesResult>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BulkPinNotesCommandHandler> _logger;

        public BulkPinNotesCommandHandler(
            INoteRepository noteRepository,
            IMapper mapper,
            ILogger<BulkPinNotesCommandHandler> logger)
        {
            _noteRepository = noteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BulkPinNotesResult> Handle(BulkPinNotesCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing bulk pin/unpin for {Count} notes by user {UserId}", 
                request.NoteIds.Count, request.UserId);

            var result = new BulkPinNotesResult();

            foreach (var noteId in request.NoteIds)
            {
                try
                {
                    // Check if user has access to the note
                    var hasAccess = await _noteRepository.HasNoteAccessAsync(noteId, request.UserId, cancellationToken);
                    if (!hasAccess)
                    {
                        _logger.LogWarning("User {UserId} attempted to pin/unpin note {NoteId} without permission", 
                            request.UserId, noteId);
                        result.FailedNoteIds.Add(noteId);
                        continue;
                    }

                    // Get the note before updating
                    var note = await _noteRepository.GetByIdAsync(noteId, 
                        n => n.User, 
                        n => n.NoteLabels, 
                        n => n.Collaborators);

                    if (note == null)
                    {
                        _logger.LogWarning("Note {NoteId} not found", noteId);
                        result.FailedNoteIds.Add(noteId);
                        continue;
                    }

                    // Update pin status
                    await _noteRepository.PinNoteAsync(noteId, request.IsPinned, cancellationToken);

                    // Get the updated note
                    var updatedNote = await _noteRepository.GetByIdAsync(noteId, 
                        n => n.User, 
                        n => n.NoteLabels, 
                        n => n.Collaborators);

                    if (updatedNote != null)
                    {
                        result.UpdatedNotes.Add(_mapper.Map<NoteDto>(updatedNote));
                        _logger.LogInformation("Successfully updated pin status for note {NoteId} to {IsPinned}", 
                            noteId, request.IsPinned);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogWarning(ex, "Unauthorized access to note {NoteId} by user {UserId}", 
                        noteId, request.UserId);
                    result.FailedNoteIds.Add(noteId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to pin/unpin note {NoteId} for user {UserId}", 
                        noteId, request.UserId);
                    result.FailedNoteIds.Add(noteId);
                }
            }

            _logger.LogInformation("Bulk pin/unpin completed: {SuccessCount} successful, {FailedCount} failed", 
                result.SuccessCount, result.FailedCount);

            return result;
        }
    }
}
