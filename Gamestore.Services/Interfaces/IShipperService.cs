using Gamestore.Entities.MongoDB;

namespace Gamestore.Services.Interfaces;

/// <summary>
/// Service interface for Shipper operations
/// E08 US1 - Get Shippers endpoint
/// </summary>
public interface IShipperService
{
    /// <summary>
    /// Gets all shippers from MongoDB with dynamic content structure
    /// </summary>
    /// <returns>Collection of shippers with free content structure</returns>
    Task<IEnumerable<object>> GetAllShippersAsync();

    /// <summary>
    /// Gets shipper by ID - zwraca object zamiast MongoShipper
    /// </summary>
    Task<object?> GetShipperByIdAsync(int shipperId);
}