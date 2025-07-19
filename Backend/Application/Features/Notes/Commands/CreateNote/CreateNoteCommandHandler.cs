using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Application.Common.Helpers;
using fundoo_notes.Domain.Entities;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Notes.Commands.CreateNote
{
    /// <summary>
    /// Handler for CreateNoteCommand
    /// </summary>
    public class CreateNoteCommandHandler : IRequestHandler<CreateNoteCommand, NoteDto>
    {
        private readonly INoteRepository _noteRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateNoteCommandHandler> _logger;

        public CreateNoteCommandHandler(
            INoteRepository noteRepository,
            ILabelRepository labelRepository,
            IMapper mapper,
            ILogger<CreateNoteCommandHandler> logger)
        {
            _noteRepository = noteRepository;
            _labelRepository = labelRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<NoteDto> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new note for user: {UserId}", request.UserId);

            // Validate and normalize color
            var normalizedColor = ColorHelper.NormalizeColor(request.Color);
            if (!ColorHelper.IsValidColor(normalizedColor))
            {
                throw new ArgumentException($"Invalid color format: {request.Color}");
            }

            // Create the note
            var note = new Note
            {
                Title = request.Title,
                Content = request.Content,
                Color = normalizedColor,
                ReminderDateTime = request.ReminderDateTime,
                UserId = request.UserId
            };

            // Add note to database
            await _noteRepository.AddAsync(note, cancellationToken);
            await _noteRepository.SaveChangesAsync(cancellationToken);

            // Add labels if provided
            if (request.LabelIds.Any())
            {
                foreach (var labelId in request.LabelIds)
                {
                    try
                    {
                        await _labelRepository.AddLabelToNoteAsync(note.Id, labelId, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to add label {LabelId} to note {NoteId}", labelId, note.Id);
                    }
                }
            }

            // Retrieve the complete note with related data
            var createdNote = await _noteRepository.GetByIdAsync(note.Id, 
                n => n.User, 
                n => n.NoteLabels, 
                n => n.Collaborators);

            if (createdNote == null)
            {
                throw new InvalidOperationException("Failed to retrieve created note");
            }

            _logger.LogInformation("Note created successfully with ID: {NoteId}", createdNote.Id);

            return _mapper.Map<NoteDto>(createdNote);
        }
    }
}
