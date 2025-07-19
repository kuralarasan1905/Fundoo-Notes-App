using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Notes.Queries.GetUserNotes
{
    /// <summary>
    /// Query for getting user notes
    /// </summary>
    public class GetUserNotesQuery : IRequest<List<NoteListDto>>
    {
        public int UserId { get; set; }
        public bool IncludeArchived { get; set; } = false;
        public bool IncludeTrashed { get; set; } = false;
    }
}
