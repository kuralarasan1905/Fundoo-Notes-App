using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Application.Common.Helpers;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Notes.Commands.UpdateNote
{
    /// <summary>
    /// Handler for UpdateNoteCommand
    /// </summary>
    public class UpdateNoteCommandHandler : IRequestHandler<UpdateNoteCommand, NoteDto>
    {
        private readonly INoteRepository _noteRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateNoteCommandHandler> _logger;

        public UpdateNoteCommandHandler(
            INoteRepository noteRepository,
            ILabelRepository labelRepository,
            IMapper mapper,
            ILogger<UpdateNoteCommandHandler> logger)
        {
            _noteRepository = noteRepository;
            _labelRepository = labelRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<NoteDto> Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating note {NoteId} for user: {UserId}", request.Id, request.UserId);

            // Get the existing note
            var note = await _noteRepository.GetByIdAsync(request.Id, cancellationToken);
            if (note == null)
            {
                throw new KeyNotFoundException($"Note with ID {request.Id} not found");
            }

            // Check if user owns the note
            if (note.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("You don't have permission to update this note");
            }

            // Validate and normalize color
            var normalizedColor = ColorHelper.NormalizeColor(request.Color);
            if (!ColorHelper.IsValidColor(normalizedColor))
            {
                throw new ArgumentException($"Invalid color format: {request.Color}");
            }

            // Update note properties
            note.Title = request.Title;
            note.Content = request.Content;
            note.Color = normalizedColor;
            note.ReminderDateTime = request.ReminderDateTime;

            // Update the note
            await _noteRepository.UpdateAsync(note, cancellationToken);
            await _noteRepository.SaveChangesAsync(cancellationToken);

            // Update labels only if LabelIds is provided and not null
            // This prevents accidental label removal when updating other note properties
            if (request.LabelIds != null)
            {
                var existingLabels = await _labelRepository.GetLabelsForNoteAsync(note.Id, cancellationToken);
                var existingLabelIds = existingLabels.Select(l => l.Id).ToList();
                var newLabelIds = request.LabelIds.ToList();

                // Remove labels that are no longer needed
                var labelsToRemove = existingLabelIds.Except(newLabelIds).ToList();
                foreach (var labelId in labelsToRemove)
                {
                    await _labelRepository.RemoveLabelFromNoteAsync(note.Id, labelId, cancellationToken);
                    _logger.LogDebug("Removed label {LabelId} from note {NoteId}", labelId, note.Id);
                }

                // Add new labels
                var labelsToAdd = newLabelIds.Except(existingLabelIds).ToList();
                foreach (var labelId in labelsToAdd)
                {
                    try
                    {
                        await _labelRepository.AddLabelToNoteAsync(note.Id, labelId, cancellationToken);
                        _logger.LogDebug("Added label {LabelId} to note {NoteId}", labelId, note.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to add label {LabelId} to note {NoteId}", labelId, note.Id);
                    }
                }

                _logger.LogInformation("Updated labels for note {NoteId}: removed {RemovedCount}, added {AddedCount}",
                    note.Id, labelsToRemove.Count, labelsToAdd.Count);
            }
            else
            {
                _logger.LogDebug("LabelIds not provided in update request for note {NoteId}, preserving existing labels", note.Id);
            }

            // Retrieve the updated note with related data
            var updatedNote = await _noteRepository.GetByIdAsync(note.Id, 
                n => n.User, 
                n => n.NoteLabels, 
                n => n.Collaborators);

            _logger.LogInformation("Note {NoteId} updated successfully", note.Id);

            return _mapper.Map<NoteDto>(updatedNote);
        }
    }
}
