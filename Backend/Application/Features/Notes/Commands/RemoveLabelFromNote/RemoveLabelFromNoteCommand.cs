using MediatR;

namespace fundoo_notes.Application.Features.Notes.Commands.RemoveLabelFromNote
{
    /// <summary>
    /// Command to remove a label from a note
    /// </summary>
    public class RemoveLabelFromNoteCommand : IRequest<bool>
    {
        public int NoteId { get; set; }
        public int LabelId { get; set; }
        public int UserId { get; set; }
    }
}
