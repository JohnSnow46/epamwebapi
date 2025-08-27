using MongoDB.Bson;

namespace Gamestore.Data.Interfaces;
/// <summary>
/// Repository interface for Order History MongoDB operations
/// Handles MongoDB data access for order history
/// </summary>
public interface IOrderHistoryRepository
{
    /// <summary>
    /// Gets all order documents from MongoDB
    /// </summary>
    Task<IEnumerable<BsonDocument>> GetAllMongoOrdersAsync();

    /// <summary>
    /// Gets filtered MongoDB orders by date range
    /// </summary>
    Task<IEnumerable<BsonDocument>> GetMongoOrdersByDateRangeAsync(DateTime? startDate, DateTime? endDate);

    /// <summary>
    /// Checks if MongoDB collection is available
    /// </summary>
    bool IsMongoAvailable();
}

