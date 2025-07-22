using Gamestore.Entities.MongoDB;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace Gamestore.Data.MongoDB;

/// <summary>
/// MongoDB context for MongoDB.Driver 2.7.3 without BsonClassMap
/// Relies on default MongoDB serialization
/// </summary>
public class MongoDbContext
{
    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
        var client = new MongoClient(connectionString);
        Database = client.GetDatabase("Northwind");
    }

    // Collections for Northwind data (read-only)
    public IMongoCollection<MongoProduct> Products => Database.GetCollection<MongoProduct>("products");
    public IMongoCollection<MongoCategory> Categories => Database.GetCollection<MongoCategory>("categories");
    public IMongoCollection<MongoSupplier> Suppliers => Database.GetCollection<MongoSupplier>("suppliers");
    public IMongoCollection<MongoOrder> Orders => Database.GetCollection<MongoOrder>("orders");
    public IMongoCollection<MongoOrderDetail> OrderDetails => Database.GetCollection<MongoOrderDetail>("orderdetails");
    public IMongoCollection<MongoShipper> Shippers => Database.GetCollection<MongoShipper>("shippers");

    // Collection for change logs (writable)
    public IMongoCollection<MongoEntityChangeLog> EntityChangeLogs => Database.GetCollection<MongoEntityChangeLog>("entitychangelogs");

    public IMongoDatabase Database { get; }
}