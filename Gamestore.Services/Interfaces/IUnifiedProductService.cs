namespace Gamestore.Services.Interfaces;

/// <summary>
/// Service interface for unified product operations across SQL and MongoDB databases
/// Implements E08 US4 requirements for CRUD operations on both databases
/// </summary>
public interface IUnifiedProductService
{
    /// <summary>
    /// Gets all products from both databases with duplicate management.
    /// </summary>
    /// <returns>Collection of unified products from both databases</returns>
    Task<IEnumerable<object>> GetAllProductsAsync();

    /// <summary>
    /// Gets a specific product by ID (searches both databases).
    /// </summary>
    /// <param name="id">Product ID to search for</param>
    /// <returns>Product data or null if not found</returns>
    Task<object?> GetProductByIdAsync(string id);

    /// <summary>
    /// Gets a product by game key (SQL) or searches MongoDB.
    /// </summary>
    /// <param name="key">Game key or product identifier</param>
    /// <returns>Product data or null if not found</returns>
    Task<object?> GetProductByKeyAsync(string key);

    /// <summary>
    /// Creates a new product (always in SQL database).
    /// </summary>
    /// <param name="productData">Product data to create</param>
    /// <returns>Created product data</returns>
    Task<object> CreateProductAsync(object productData);

    /// <summary>
    /// Updates a product (handles both databases according to E08 US7 rules).
    /// </summary>
    /// <param name="id">Product ID to update</param>
    /// <param name="productData">Updated product data</param>
    /// <returns>Updated product data</returns>
    Task<object> UpdateProductAsync(string id, object productData);

    /// <summary>
    /// Synchronizes stock count across databases.
    /// </summary>
    /// <param name="productId">Product ID to sync</param>
    /// <param name="newStock">New stock count</param>
    /// <returns>Synchronization result</returns>
    Task<object> SyncStockCountAsync(string productId, int newStock);

    /// <summary>
    /// Deletes a product (only from SQL database).
    /// </summary>
    /// <param name="id">Product ID to delete</param>
    /// <returns>True if deleted successfully, false otherwise</returns>
    Task<bool> DeleteProductAsync(string id);
}