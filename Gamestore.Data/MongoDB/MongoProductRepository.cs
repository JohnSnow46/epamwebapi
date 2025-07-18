using Gamestore.Data.Interfaces;
using Gamestore.Entities.MongoDB;
using MongoDB.Driver;

namespace Gamestore.Data.MongoDB;

/// <summary>
/// Specific MongoDB repository for Product operations
/// Implements product-specific business logic
/// </summary>
public class MongoProductRepository(IMongoCollection<MongoProduct> collection) : MongoRepository<MongoProduct>(collection), IMongoProductRepository
{
    public async Task<MongoProduct?> GetByGameKeyAsync(string gameKey)
    {
        return await GetFirstOrDefaultAsync(p => p.GameKey == gameKey);
    }

    public async Task<IEnumerable<MongoProduct>> GetByCategoryIdAsync(int categoryId)
    {
        return await GetAsync(p => p.CategoryId == categoryId);
    }

    public async Task<IEnumerable<MongoProduct>> GetBySupplierIdAsync(int supplierId)
    {
        return await GetAsync(p => p.SupplierId == supplierId);
    }

    public async Task<bool> UpdateGameKeyAsync(int productId, string gameKey)
    {
        var filter = Builders<MongoProduct>.Filter.Eq(p => p.ProductId, productId);
        var update = Builders<MongoProduct>.Update.Set(p => p.GameKey, gameKey);
        return await UpdateAsync(filter, update);
    }

    public async Task<bool> IncrementViewCountAsync(int productId)
    {
        var filter = Builders<MongoProduct>.Filter.Eq(p => p.ProductId, productId);
        var update = Builders<MongoProduct>.Update.Inc(p => p.ViewCount, 1);
        return await UpdateAsync(filter, update);
    }

    public async Task<IEnumerable<MongoProduct>> GetAvailableProductsAsync()
    {
        return await GetAsync(p => !p.Discontinued && p.UnitsInStock > 0);
    }
}