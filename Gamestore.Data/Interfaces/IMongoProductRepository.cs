using Gamestore.Entities.MongoDB;

namespace Gamestore.Data.Interfaces;

/// <summary>
/// Specific repository interface for MongoDB Product operations
/// Extends generic repository with product-specific methods
/// </summary>
public interface IMongoProductRepository : IMongoRepository<MongoProduct>
{
    Task<MongoProduct?> GetByGameKeyAsync(string gameKey);
    Task<IEnumerable<MongoProduct>> GetByCategoryIdAsync(int categoryId);
    Task<IEnumerable<MongoProduct>> GetBySupplierIdAsync(int supplierId);
    Task<bool> UpdateGameKeyAsync(int productId, string gameKey);
    Task<bool> IncrementViewCountAsync(int productId);
    Task<IEnumerable<MongoProduct>> GetAvailableProductsAsync();
}