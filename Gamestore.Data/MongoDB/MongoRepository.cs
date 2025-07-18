using Gamestore.Data.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace Gamestore.Data.MongoDB;

/// <summary>
/// Generic MongoDB repository implementation
/// Implements all NFR requirements for MongoDB operations
/// </summary>
public class MongoRepository<T>(IMongoCollection<T> collection) : IMongoRepository<T> where T : class
{
#pragma warning disable CA1051 // Do not declare visible instance fields
    protected readonly IMongoCollection<T> _collection = collection;
#pragma warning restore CA1051 // Do not declare visible instance fields

    // E08 NFR1 - IQueryable for read operations
    public IQueryable<T> AsQueryable()
    {
        return _collection.AsQueryable();
    }

    public async Task<T?> GetByIdAsync(object id)
    {
        var filter = Builders<T>.Filter.Eq("_id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> filter)
    {
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter)
    {
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    // E08 NFR2 - FilterDefinition/UpdateDefinition for updates
    public async Task<bool> UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update)
    {
        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateManyAsync(FilterDefinition<T> filter, UpdateDefinition<T> update)
    {
        var result = await _collection.UpdateManyAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    // E08 NFR3 - Raw insert/delete operations
    public async Task InsertOneAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task InsertManyAsync(IEnumerable<T> entities)
    {
        await _collection.InsertManyAsync(entities);
    }

    public async Task<bool> DeleteAsync(FilterDefinition<T> filter)
    {
        var result = await _collection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<bool> DeleteManyAsync(FilterDefinition<T> filter)
    {
        var result = await _collection.DeleteManyAsync(filter);
        return result.DeletedCount > 0;
    }

    // Additional operations
    public async Task<long> CountAsync(FilterDefinition<T> filter)
    {
        return await _collection.CountDocumentsAsync(filter);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
    {
        return await _collection.Find(filter).AnyAsync();
    }
}