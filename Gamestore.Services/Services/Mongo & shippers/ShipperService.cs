using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Gamestore.Services.Services;

/// <summary>
/// Service for Shipper operations from MongoDB.
/// Uses direct connection instead of complex repositories for simplified operations.
/// </summary>
public class ShipperService : IShipperService
{
    private readonly ILogger<ShipperService> _logger;
    private readonly IMongoCollection<BsonDocument> _shippersCollection;

    public ShipperService(ILogger<ShipperService> logger)
    {
        _logger = logger;

        var client = new MongoClient("mongodb://localhost:27017");
        var database = client.GetDatabase("Northwind");
        _shippersCollection = database.GetCollection<BsonDocument>("shippers");
    }

    /// <summary>
    /// Gets all shippers with dynamic content structure as per E08 US1
    /// </summary>
    public async Task<IEnumerable<object>> GetAllShippersAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all shippers from MongoDB");

            var documents = await _shippersCollection.Find(new BsonDocument()).ToListAsync();

            _logger.LogInformation("Found {Count} shippers", documents.Count);

            var result = documents.Select(doc => new
            {
                shipperId = doc.Contains("ShipperID") ? doc["ShipperID"].ToInt32() : 0,
                companyName = doc.Contains("CompanyName") ? doc["CompanyName"].AsString : "N/A",
                phone = doc.Contains("Phone") ? doc["Phone"].AsString : "N/A",
                mongoId = doc["_id"].ToString()
            });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching shippers from MongoDB");
            throw;
        }
    }

    /// <summary>
    /// Gets shipper by ID with simplified implementation.
    /// </summary>
    /// <param name="shipperId">The shipper ID to search for</param>
    /// <returns>Shipper data or null if not found</returns>
    public async Task<object?> GetShipperByIdAsync(int shipperId)
    {
        try
        {
            _logger.LogInformation("Fetching shipper with ID: {ShipperId}", shipperId);

            var filter = Builders<BsonDocument>.Filter.Eq("ShipperID", shipperId);
            var document = await _shippersCollection.Find(filter).FirstOrDefaultAsync();

            if (document == null)
            {
                _logger.LogWarning("Shipper with ID {ShipperId} not found", shipperId);
                return null;
            }

            return new
            {
                shipperId = document["ShipperID"].ToInt32(),
                companyName = document["CompanyName"].AsString,
                phone = document["Phone"].AsString,
                mongoId = document["_id"].ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching shipper {ShipperId} from MongoDB", shipperId);
            throw;
        }
    }
}