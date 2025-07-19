using Microsoft.EntityFrameworkCore;
using fundoo_notes.Domain.Entities;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Infrastructure.Data;

namespace fundoo_notes.Infrastructure.Repositories
{
    /// <summary>
    /// Attachment repository implementation with attachment-specific operations
    /// </summary>
    public class AttachmentRepository : GenericRepository<NoteAttachment>, IAttachmentRepository
    {
        public AttachmentRepository(FundooNotesDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<NoteAttachment>> GetByNoteIdAsync(int noteId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(a => a.NoteId == noteId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<NoteAttachment?> GetByIdWithNoteAsync(int attachmentId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(a => a.Note)
                .FirstOrDefaultAsync(a => a.Id == attachmentId, cancellationToken);
        }
    }
}
