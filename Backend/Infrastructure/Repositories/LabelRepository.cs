using Microsoft.EntityFrameworkCore;
using fundoo_notes.Domain.Entities;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Infrastructure.Data;

namespace fundoo_notes.Infrastructure.Repositories
{
    /// <summary>
    /// Label repository implementation with label-specific operations
    /// </summary>
    public class LabelRepository : GenericRepository<Label>, ILabelRepository
    {
        public LabelRepository(FundooNotesDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Label>> GetUserLabelsAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(l => l.UserId == userId)
                .OrderBy(l => l.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<Label?> GetUserLabelByNameAsync(int userId, string labelName, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(l => l.UserId == userId && l.Name == labelName, cancellationToken);
        }

        public async Task<bool> LabelExistsForUserAsync(int userId, string labelName, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(l => l.UserId == userId && l.Name == labelName, cancellationToken);
        }

        public async Task<IEnumerable<Label>> GetLabelsForNoteAsync(int noteId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(l => l.NoteLabels.Any(nl => nl.NoteId == noteId))
                .OrderBy(l => l.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task AddLabelToNoteAsync(int noteId, int labelId, CancellationToken cancellationToken = default)
        {
            var existingNoteLabel = await _context.NoteLabels
                .FirstOrDefaultAsync(nl => nl.NoteId == noteId && nl.LabelId == labelId, cancellationToken);

            if (existingNoteLabel == null)
            {
                var noteLabel = new NoteLabel
                {
                    NoteId = noteId,
                    LabelId = labelId
                };

                await _context.NoteLabels.AddAsync(noteLabel, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task RemoveLabelFromNoteAsync(int noteId, int labelId, CancellationToken cancellationToken = default)
        {
            var noteLabel = await _context.NoteLabels
                .FirstOrDefaultAsync(nl => nl.NoteId == noteId && nl.LabelId == labelId, cancellationToken);

            if (noteLabel != null)
            {
                _context.NoteLabels.Remove(noteLabel);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task RemoveAllNoteLabelAssociationsAsync(int labelId, CancellationToken cancellationToken = default)
        {
            var noteLabels = await _context.NoteLabels
                .Where(nl => nl.LabelId == labelId)
                .ToListAsync(cancellationToken);

            if (noteLabels.Any())
            {
                _context.NoteLabels.RemoveRange(noteLabels);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<int> GetLabelUsageCountAsync(int labelId, CancellationToken cancellationToken = default)
        {
            return await _context.NoteLabels
                .CountAsync(nl => nl.LabelId == labelId, cancellationToken);
        }
    }
}
