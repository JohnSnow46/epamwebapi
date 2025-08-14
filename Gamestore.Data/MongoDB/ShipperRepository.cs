using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gamestore.Data.MongoDB;

public class ShipperRepository : IShipperRepository
{
    private readonly ILogger<ShipperRepository> _logger;
    private readonly IMongoCollection<BsonDocument> _shippersCollection;

    public ShipperRepository(ILogger<ShipperRepository> logger, IConfiguration configuration)
    {
        _logger = logger;

        // MongoDB connection setup
        var connectionString = configuration.GetConnectionString("MongoDb") ?? "mongodb://localhost:27017";
        var databaseName = configuration["MongoDb:DatabaseName"] ?? "Northwind";

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _shippersCollection = database.GetCollection<BsonDocument>("shippers");
    }

    public async Task<IEnumerable<BsonDocument>> GetAllShippersAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all shippers from MongoDB");
            return await _shippersCollection.Find(new BsonDocument()).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching shippers from MongoDB");
            throw;
        }
    }

    public async Task<BsonDocument?> GetShipperByIdAsync(int shipperId)
    {
        try
        {
            _logger.LogInformation("Fetching shipper with ID: {ShipperId}", shipperId);
            var filter = Builders<BsonDocument>.Filter.Eq("ShipperID", shipperId);
            return await _shippersCollection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching shipper {ShipperId} from MongoDB", shipperId);
            throw;
        }
    }
}