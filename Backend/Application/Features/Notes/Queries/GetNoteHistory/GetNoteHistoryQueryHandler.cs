using MediatR;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Interfaces;
using AutoMapper;

namespace fundoo_notes.Application.Features.Notes.Queries.GetNoteHistory
{
    public class GetNoteHistoryQueryHandler : IRequestHandler<GetNoteHistoryQuery, List<NoteHistoryDto>>
    {
        private readonly INoteHistoryRepository _noteHistoryRepository;
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;

        public GetNoteHistoryQueryHandler(
            INoteHistoryRepository noteHistoryRepository,
            INoteRepository noteRepository,
            IMapper mapper)
        {
            _noteHistoryRepository = noteHistoryRepository;
            _noteRepository = noteRepository;
            _mapper = mapper;
        }

        public async Task<List<NoteHistoryDto>> Handle(GetNoteHistoryQuery request, CancellationToken cancellationToken)
        {
            // Verify user has access to the note
            var note = await _noteRepository.GetByIdAsync(request.NoteId);
            if (note == null || note.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            var history = await _noteHistoryRepository.GetByNoteIdAsync(request.NoteId);
            return _mapper.Map<List<NoteHistoryDto>>(history);
        }
    }
}
