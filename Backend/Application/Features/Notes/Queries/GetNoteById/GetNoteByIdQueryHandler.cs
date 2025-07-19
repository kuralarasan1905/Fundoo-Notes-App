using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Notes.Queries.GetNoteById
{
    /// <summary>
    /// Handler for GetNoteByIdQuery
    /// </summary>
    public class GetNoteByIdQueryHandler : IRequestHandler<GetNoteByIdQuery, NoteDto?>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetNoteByIdQueryHandler> _logger;

        public GetNoteByIdQueryHandler(
            INoteRepository noteRepository,
            IMapper mapper,
            ILogger<GetNoteByIdQueryHandler> logger)
        {
            _noteRepository = noteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<NoteDto?> Handle(GetNoteByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting note {NoteId} for user: {UserId}", request.Id, request.UserId);

            // Get the note with related data
            var note = await _noteRepository.GetByIdAsync(request.Id, 
                n => n.User, 
                n => n.NoteLabels, 
                n => n.Collaborators);

            if (note == null)
            {
                _logger.LogWarning("Note {NoteId} not found", request.Id);
                return null;
            }

            // Check if user owns the note or is a collaborator
            if (note.UserId != request.UserId)
            {
                // Check if user is a collaborator
                var isCollaborator = note.Collaborators?.Any(c => c.UserId == request.UserId && c.IsAccepted) ?? false;
                if (!isCollaborator)
                {
                    _logger.LogWarning("User {UserId} attempted to access note {NoteId} without permission", request.UserId, request.Id);
                    throw new UnauthorizedAccessException("You don't have permission to view this note");
                }
            }

            _logger.LogInformation("Note {NoteId} retrieved successfully for user: {UserId}", request.Id, request.UserId);

            return _mapper.Map<NoteDto>(note);
        }
    }
}
