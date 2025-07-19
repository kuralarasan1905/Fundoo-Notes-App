using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using fundoo_notes.Domain.Entities;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Infrastructure.Data;

namespace fundoo_notes.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository implementation providing common CRUD operations
    /// </summary>
    /// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly FundooNotesDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(FundooNotesDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public virtual async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return await query.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return await query.Where(predicate).ToListAsync();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
                return await _dbSet.CountAsync(cancellationToken);

            return await _dbSet.CountAsync(predicate, cancellationToken);
        }

        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            Expression<Func<T, object>>? orderBy = null,
            bool ascending = true,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            if (predicate != null)
                query = query.Where(predicate);

            var totalCount = await query.CountAsync(cancellationToken);

            if (orderBy != null)
            {
                query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            }

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
            return entities;
        }

        public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Update(entity);
            return await Task.FromResult(entity);
        }

        public virtual async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            _dbSet.UpdateRange(entities);
            return await Task.FromResult(entities);
        }

        public virtual async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            _dbSet.RemoveRange(entities);
            await Task.CompletedTask;
        }

        public virtual async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
                await UpdateAsync(entity, cancellationToken);
            }
        }

        public virtual async Task SoftDeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            await UpdateAsync(entity, cancellationToken);
        }

        public virtual async Task RestoreAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity != null)
            {
                entity.IsDeleted = false;
                entity.DeletedAt = null;
                await UpdateAsync(entity, cancellationToken);
            }
        }

        public virtual async Task RestoreAsync(T entity, CancellationToken cancellationToken = default)
        {
            entity.IsDeleted = false;
            entity.DeletedAt = null;
            await UpdateAsync(entity, cancellationToken);
        }

        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
