using MediatR;
using fundoo_notes.Application.DTOs;

namespace fundoo_notes.Application.Features.Notes.Commands.ManageNoteLabels
{
    /// <summary>
    /// Command to manage all labels for a note (replace existing labels with new set)
    /// </summary>
    public class ManageNoteLabelsCommand : IRequest<NoteDto>
    {
        public int NoteId { get; set; }
        public List<int> LabelIds { get; set; } = new List<int>();
        public int UserId { get; set; }
    }
}
