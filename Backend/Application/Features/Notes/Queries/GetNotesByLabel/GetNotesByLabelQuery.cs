using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Notes.Queries.GetNotesByLabel
{
    /// <summary>
    /// Query for getting notes filtered by label
    /// </summary>
    public class GetNotesByLabelQuery : IRequest<List<NoteListDto>>
    {
        public int LabelId { get; set; }
        public int UserId { get; set; } // Set by the controller from JWT token
    }
}
