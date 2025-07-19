using MediatR;

namespace fundoo_notes.Application.Features.Notes.Commands.BulkRemoveLabel
{
    /// <summary>
    /// Command to remove a label from multiple notes
    /// </summary>
    public class BulkRemoveLabelCommand : IRequest<BulkRemoveLabelResult>
    {
        public List<int> NoteIds { get; set; } = new List<int>();
        public int LabelId { get; set; }
        public int UserId { get; set; }
    }

    public class BulkRemoveLabelResult
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<int> ProcessedNoteIds { get; set; } = new List<int>();
    }
}
