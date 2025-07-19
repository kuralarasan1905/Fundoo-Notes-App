using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Notes.Queries.GetUserNotes
{
    /// <summary>
    /// Handler for GetUserNotesQuery
    /// </summary>
    public class GetUserNotesQueryHandler : IRequestHandler<GetUserNotesQuery, List<NoteListDto>>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetUserNotesQueryHandler> _logger;

        public GetUserNotesQueryHandler(
            INoteRepository noteRepository,
            IMapper mapper,
            ILogger<GetUserNotesQueryHandler> logger)
        {
            _noteRepository = noteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<NoteListDto>> Handle(GetUserNotesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting notes for user: {UserId}", request.UserId);

            var notes = await _noteRepository.GetUserNotesAsync(
                request.UserId, 
                request.IncludeArchived, 
                request.IncludeTrashed, 
                cancellationToken);

            var noteDtos = _mapper.Map<List<NoteListDto>>(notes);

            _logger.LogInformation("Retrieved {Count} notes for user: {UserId}", noteDtos.Count, request.UserId);

            return noteDtos;
        }
    }
}
