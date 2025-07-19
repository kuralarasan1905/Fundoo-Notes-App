using Microsoft.EntityFrameworkCore;
using fundoo_notes.Domain.Entities;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Infrastructure.Data;

namespace fundoo_notes.Infrastructure.Repositories
{
    /// <summary>
    /// Note template repository implementation with template-specific operations
    /// </summary>
    public class NoteTemplateRepository : GenericRepository<NoteTemplate>, INoteTemplateRepository
    {
        public NoteTemplateRepository(FundooNotesDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<NoteTemplate>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<NoteTemplate>> GetPublicTemplatesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.IsPublic)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<NoteTemplate?> GetByIdAndUserAsync(int templateId, int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Id == templateId && t.UserId == userId, cancellationToken);
        }
    }
}
