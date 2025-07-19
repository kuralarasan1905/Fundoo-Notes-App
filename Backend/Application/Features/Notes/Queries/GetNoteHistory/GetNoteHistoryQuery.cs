using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Notes.Queries.GetNoteHistory
{
    public class GetNoteHistoryQuery : IRequest<List<NoteHistoryDto>>
    {
        public int NoteId { get; set; }
        public int UserId { get; set; }
    }
}
