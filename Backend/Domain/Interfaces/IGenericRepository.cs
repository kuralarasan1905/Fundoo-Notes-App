using System.Linq.Expressions;
using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Domain.Interfaces
{
    /// <summary>
    /// Generic repository interface providing common CRUD operations
    /// </summary>
    /// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
    public interface IGenericRepository<T> where T : BaseEntity
    {
        // Query operations
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
        
        // Paging operations
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            Expression<Func<T, bool>>? predicate = null,
            Expression<Func<T, object>>? orderBy = null,
            bool ascending = true,
            CancellationToken cancellationToken = default);
        
        // Command operations
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        
        // Soft delete operations
        Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
        Task SoftDeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task RestoreAsync(int id, CancellationToken cancellationToken = default);
        Task RestoreAsync(T entity, CancellationToken cancellationToken = default);
        
        // Transaction support
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
