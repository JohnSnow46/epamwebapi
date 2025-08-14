using Gamestore.Data.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Gamestore.Data.MongoDB;

/// <summary>
/// Repository for Order History MongoDB operations
/// Handles only MongoDB data access - no business logic
/// </summary>
public class OrderHistoryRepository : IOrderHistoryRepository
{
    private readonly ILogger<OrderHistoryRepository> _logger;
    private readonly IMongoCollection<BsonDocument>? _mongoOrdersCollection;

    public OrderHistoryRepository(ILogger<OrderHistoryRepository> logger, IConfiguration configuration)
    {
        _logger = logger;

        try
        {
            var connectionString = configuration.GetConnectionString("MongoDb") ?? "mongodb://localhost:27017";
            var databaseName = configuration["MongoDb:DatabaseName"] ?? "Northwind";

            _logger.LogInformation("Connecting to MongoDB: {ConnectionString}, Database: {DatabaseName}",
                connectionString, databaseName);

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _mongoOrdersCollection = database.GetCollection<BsonDocument>("orders");

            _logger.LogInformation("MongoDB connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish MongoDB connection");
            _mongoOrdersCollection = null;
        }
    }

    /// <summary>
    /// Gets all order documents from MongoDB
    /// Pure data access - no filtering/processing
    /// </summary>
    public async Task<IEnumerable<BsonDocument>> GetAllMongoOrdersAsync()
    {
        if (!IsMongoAvailable())
        {
            _logger.LogWarning("MongoDB collection is not available, returning empty collection");
            return Enumerable.Empty<BsonDocument>();
        }

        try
        {
            _logger.LogDebug("Fetching all orders from MongoDB repository");
            var documents = await _mongoOrdersCollection!.Find(new BsonDocument()).ToListAsync();
            _logger.LogDebug("Retrieved {Count} documents from MongoDB", documents.Count);
            return documents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders from MongoDB in repository");
            return Enumerable.Empty<BsonDocument>();
        }
    }

    /// <summary>
    /// Gets filtered MongoDB orders by date range
    /// Repository can handle basic filtering
    /// </summary>
    public async Task<IEnumerable<BsonDocument>> GetMongoOrdersByDateRangeAsync(DateTime? startDate, DateTime? endDate)
    {
        if (!IsMongoAvailable())
        {
            return Enumerable.Empty<BsonDocument>();
        }

        try
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Empty;

            // Basic date filtering at repository level
            if (startDate.HasValue || endDate.HasValue)
            {
                var dateFilters = new List<FilterDefinition<BsonDocument>>();

                if (startDate.HasValue)
                {
                    dateFilters.Add(filterBuilder.Gte("OrderDate", startDate.Value.ToString("yyyy-MM-dd")));
                }

                if (endDate.HasValue)
                {
                    dateFilters.Add(filterBuilder.Lte("OrderDate", endDate.Value.ToString("yyyy-MM-dd")));
                }

                filter = filterBuilder.And(dateFilters);
            }

            _logger.LogDebug("Fetching MongoDB orders with date filter - Start: {StartDate}, End: {EndDate}",
                startDate, endDate);

            var documents = await _mongoOrdersCollection!.Find(filter).ToListAsync();
            _logger.LogDebug("Retrieved {Count} filtered documents from MongoDB", documents.Count);
            return documents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching filtered orders from MongoDB in repository");
            return Enumerable.Empty<BsonDocument>();
        }
    }

    /// <summary>
    /// Checks if MongoDB collection is available
    /// </summary>
    public bool IsMongoAvailable()
    {
        return _mongoOrdersCollection != null;
    }
}