using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Application.Features.Notes.Queries.SearchNotes
{
    /// <summary>
    /// Handler for SearchNotesQuery
    /// </summary>
    public class SearchNotesQueryHandler : IRequestHandler<SearchNotesQuery, List<NoteListDto>>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SearchNotesQueryHandler> _logger;

        public SearchNotesQueryHandler(
            INoteRepository noteRepository,
            IMapper mapper,
            ILogger<SearchNotesQueryHandler> logger)
        {
            _noteRepository = noteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<NoteListDto>> Handle(SearchNotesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Searching notes for user {UserId} with term: {SearchTerm}", request.UserId, request.SearchTerm);

            // Use database-level search instead of in-memory filtering
            var notes = await _noteRepository.SearchNotesAsync(
                request.UserId,
                request.SearchTerm,
                cancellationToken);

            // Apply additional filters if needed
            var filteredNotes = notes.AsEnumerable();

            if (!request.IncludeArchived)
                filteredNotes = filteredNotes.Where(n => !n.IsArchived);

            if (!request.IncludeTrashed)
                filteredNotes = filteredNotes.Where(n => !n.IsTrashed);

            var resultList = filteredNotes.ToList();

            _logger.LogInformation("Found {Count} notes matching search term '{SearchTerm}' for user: {UserId}",
                resultList.Count, request.SearchTerm, request.UserId);

            return _mapper.Map<List<NoteListDto>>(resultList);
        }
    }
}
