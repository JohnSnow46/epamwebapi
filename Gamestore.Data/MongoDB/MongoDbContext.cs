using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;

namespace Gamestore.Data.MongoDB;

/// <summary>
/// MongoDB context using BsonDocument to avoid BsonClassMap issues
/// Workaround for MongoDB.Driver 2.7.3 + .NET 8 compatibility
/// </summary>
public class MongoDbContext
{
    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
        var client = new MongoClient(connectionString);
        Database = client.GetDatabase("Northwind");
    }

    // Use BsonDocument collections to avoid BsonClassMap initialization
    public IMongoCollection<BsonDocument> ProductsRaw => Database.GetCollection<BsonDocument>("products");
    public IMongoCollection<BsonDocument> CategoriesRaw => Database.GetCollection<BsonDocument>("categories");
    public IMongoCollection<BsonDocument> SuppliersRaw => Database.GetCollection<BsonDocument>("suppliers");
    public IMongoCollection<BsonDocument> OrdersRaw => Database.GetCollection<BsonDocument>("orders");
    public IMongoCollection<BsonDocument> OrderDetailsRaw => Database.GetCollection<BsonDocument>("orderdetails");
    public IMongoCollection<BsonDocument> ShippersRaw => Database.GetCollection<BsonDocument>("shippers");
    public IMongoCollection<BsonDocument> EntityChangeLogsRaw => Database.GetCollection<BsonDocument>("entitychangelogs");

    public IMongoDatabase Database { get; }
}