using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Application.Features.Notes.Commands.ManageNoteLabels
{
    /// <summary>
    /// Handler for managing all labels for a note
    /// </summary>
    public class ManageNoteLabelsCommandHandler : IRequestHandler<ManageNoteLabelsCommand, NoteDto>
    {
        private readonly INoteRepository _noteRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly ILogger<ManageNoteLabelsCommandHandler> _logger;

        public ManageNoteLabelsCommandHandler(
            INoteRepository noteRepository,
            ILabelRepository labelRepository,
            ILogger<ManageNoteLabelsCommandHandler> logger)
        {
            _noteRepository = noteRepository;
            _labelRepository = labelRepository;
            _logger = logger;
        }

        public async Task<NoteDto> Handle(ManageNoteLabelsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Managing labels for note {NoteId} for user: {UserId}", 
                request.NoteId, request.UserId);

            // Verify note exists and belongs to user
            var note = await _noteRepository.GetByIdAsync(request.NoteId, cancellationToken);
            if (note == null || note.UserId != request.UserId)
            {
                throw new KeyNotFoundException($"Note with ID {request.NoteId} not found");
            }

            // Verify all labels exist and belong to user
            var labels = new List<Label>();
            foreach (var labelId in request.LabelIds)
            {
                var label = await _labelRepository.GetByIdAsync(labelId, cancellationToken);
                if (label == null || label.UserId != request.UserId)
                {
                    throw new KeyNotFoundException($"Label with ID {labelId} not found");
                }
                labels.Add(label);
            }

            // Get current label associations
            var currentLabelIds = note.NoteLabels.Select(nl => nl.LabelId).ToList();

            // Remove labels that are no longer needed
            var labelsToRemove = currentLabelIds.Except(request.LabelIds).ToList();
            foreach (var labelId in labelsToRemove)
            {
                await _labelRepository.RemoveLabelFromNoteAsync(request.NoteId, labelId, cancellationToken);
                _logger.LogDebug("Removed label {LabelId} from note {NoteId}", labelId, request.NoteId);
            }

            // Add new labels
            var labelsToAdd = request.LabelIds.Except(currentLabelIds).ToList();
            foreach (var labelId in labelsToAdd)
            {
                await _labelRepository.AddLabelToNoteAsync(request.NoteId, labelId, cancellationToken);
                _logger.LogDebug("Added label {LabelId} to note {NoteId}", labelId, request.NoteId);
            }

            // Refresh note data to get updated labels
            note = await _noteRepository.GetByIdAsync(request.NoteId, cancellationToken);

            _logger.LogInformation("Successfully managed labels for note {NoteId}. Added: {AddedCount}, Removed: {RemovedCount}", 
                request.NoteId, labelsToAdd.Count, labelsToRemove.Count);

            // Return updated note
            return new NoteDto
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                Color = note.Color,
                IsPinned = note.IsPinned,
                IsArchived = note.IsArchived,
                IsTrashed = note.IsTrashed,
                ReminderDateTime = note.ReminderDateTime,
                UserId = note.UserId,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt,
                Labels = labels.Select(l => new LabelDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Color = l.Color,
                    UserId = l.UserId,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt
                }).ToList()
            };
        }
    }
}
