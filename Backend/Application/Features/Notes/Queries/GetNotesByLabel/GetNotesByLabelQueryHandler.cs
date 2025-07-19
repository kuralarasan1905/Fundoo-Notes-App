using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Notes.Queries.GetNotesByLabel
{
    /// <summary>
    /// Handler for GetNotesByLabelQuery
    /// </summary>
    public class GetNotesByLabelQueryHandler : IRequestHandler<GetNotesByLabelQuery, List<NoteListDto>>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetNotesByLabelQueryHandler> _logger;

        public GetNotesByLabelQueryHandler(
            INoteRepository noteRepository,
            IMapper mapper,
            ILogger<GetNotesByLabelQueryHandler> logger)
        {
            _noteRepository = noteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<NoteListDto>> Handle(GetNotesByLabelQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting notes by label {LabelId} for user: {UserId}", request.LabelId, request.UserId);

            var notes = await _noteRepository.GetNotesByLabelAsync(
                request.UserId, 
                request.LabelId, 
                cancellationToken);

            var noteDtos = _mapper.Map<List<NoteListDto>>(notes);

            _logger.LogInformation("Retrieved {Count} notes for label {LabelId} and user: {UserId}", 
                noteDtos.Count, request.LabelId, request.UserId);

            return noteDtos;
        }
    }
}
