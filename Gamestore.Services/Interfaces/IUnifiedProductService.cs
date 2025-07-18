namespace Gamestore.Services.Interfaces;

/// <summary>
/// Service interface for unified product operations across SQL and MongoDB databases
/// Implements E08 US4 requirements for CRUD operations on both databases
/// </summary>
public interface IUnifiedProductService
{
    /// <summary>
    /// Gets all products from both databases with duplicate management
    /// </summary>
    Task<IEnumerable<object>> GetAllProductsAsync();

    /// <summary>
    /// Gets a specific product by ID (searches both databases)
    /// </summary>
    Task<object?> GetProductByIdAsync(string id);

    /// <summary>
    /// Gets a product by game key (SQL) or searches MongoDB
    /// </summary>
    Task<object?> GetProductByKeyAsync(string key);

    /// <summary>
    /// Creates a new product (always in SQL database)
    /// </summary>
    Task<object> CreateProductAsync(object productData);

    /// <summary>
    /// Updates a product (handles both databases according to E08 US7 rules)
    /// </summary>
    Task<object> UpdateProductAsync(string id, object productData);

    Task<object> SyncStockCountAsync(string productId, int newStock);

    /// <summary>
    /// Deletes a product (only from SQL database)
    /// </summary>
    Task<bool> DeleteProductAsync(string id);
}