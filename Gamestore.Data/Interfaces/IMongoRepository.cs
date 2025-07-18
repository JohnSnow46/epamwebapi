using MongoDB.Driver;
using System.Linq.Expressions;

namespace Gamestore.Data.Interfaces;

/// <summary>
/// Generic interface for MongoDB repository operations
/// Supports IQueryable for reads and FilterDefinition/UpdateDefinition for updates (NFR1, NFR2)
/// </summary>
public interface IMongoRepository<T> where T : class
{
    // Read operations using IQueryable (E08 NFR1)
    IQueryable<T> AsQueryable();
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> filter);
    Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter);

    // Update operations using FilterDefinition/UpdateDefinition (E08 NFR2)
    Task<bool> UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update);
    Task<bool> UpdateManyAsync(FilterDefinition<T> filter, UpdateDefinition<T> update);

    // Raw insert/delete operations (E08 NFR3)
    Task InsertOneAsync(T entity);
    Task InsertManyAsync(IEnumerable<T> entities);
    Task<bool> DeleteAsync(FilterDefinition<T> filter);
    Task<bool> DeleteManyAsync(FilterDefinition<T> filter);

    // Advanced operations
    Task<long> CountAsync(FilterDefinition<T> filter);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);
}