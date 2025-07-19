using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Notes.Queries.SearchNotes
{
    /// <summary>
    /// Query for searching notes by title and content
    /// </summary>
    public class SearchNotesQuery : IRequest<List<NoteListDto>>
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int UserId { get; set; } // Set by the controller from JWT token
        public bool IncludeArchived { get; set; } = false;
        public bool IncludeTrashed { get; set; } = false;
    }
}
