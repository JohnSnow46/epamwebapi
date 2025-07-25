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
    /// Gets shipper by ID - returns object instead of MongoShipper.
    /// </summary>
    /// <param name="shipperId">The shipper ID to search for</param>
    /// <returns>Shipper data or null if not found</returns>
    Task<object?> GetShipperByIdAsync(int shipperId);
}