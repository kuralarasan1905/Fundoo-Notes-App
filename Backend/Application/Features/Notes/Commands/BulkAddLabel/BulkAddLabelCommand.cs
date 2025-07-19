using MediatR;

namespace fundoo_notes.Application.Features.Notes.Commands.BulkAddLabel
{
    /// <summary>
    /// Command to add a label to multiple notes
    /// </summary>
    public class BulkAddLabelCommand : IRequest<BulkAddLabelResult>
    {
        public List<int> NoteIds { get; set; } = new List<int>();
        public int LabelId { get; set; }
        public int UserId { get; set; }
    }

    public class BulkAddLabelResult
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<int> ProcessedNoteIds { get; set; } = new List<int>();
    }
}
