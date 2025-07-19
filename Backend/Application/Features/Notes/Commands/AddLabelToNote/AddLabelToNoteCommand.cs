using MediatR;

namespace fundoo_notes.Application.Features.Notes.Commands.AddLabelToNote
{
    /// <summary>
    /// Command to add a label to a note
    /// </summary>
    public class AddLabelToNoteCommand : IRequest<bool>
    {
        public int NoteId { get; set; }
        public int LabelId { get; set; }
        public int UserId { get; set; }
    }
}
