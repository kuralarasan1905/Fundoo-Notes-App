using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Domain.Interfaces
{
    /// <summary>
    /// Attachment repository interface with attachment-specific operations
    /// </summary>
    public interface IAttachmentRepository : IGenericRepository<NoteAttachment>
    {
        Task<IEnumerable<NoteAttachment>> GetByNoteIdAsync(int noteId, CancellationToken cancellationToken = default);
        Task<NoteAttachment?> GetByIdWithNoteAsync(int attachmentId, CancellationToken cancellationToken = default);
    }
}
