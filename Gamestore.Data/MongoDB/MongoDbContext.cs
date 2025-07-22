using Gamestore.Entities.MongoDB;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization;

namespace Gamestore.Data.MongoDB;

/// <summary>
/// MongoDB context for Northwind database operations
/// Provides access to all Northwind collections with proper configuration
/// </summary>
public class MongoDbContext
{
    static MongoDbContext()
    {
        // Initialize BsonClassMap for entities to prevent TypeInitializationException
        // This is required for MongoDB.Driver 2.7.3 with .NET 8 nullable context
        try
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(MongoProduct)))
            {
                BsonClassMap.RegisterClassMap<MongoProduct>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof(MongoCategory)))
            {
                BsonClassMap.RegisterClassMap<MongoCategory>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof(MongoSupplier)))
            {
                BsonClassMap.RegisterClassMap<MongoSupplier>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof(MongoOrder)))
            {
                BsonClassMap.RegisterClassMap<MongoOrder>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof(MongoOrderDetail)))
            {
                BsonClassMap.RegisterClassMap<MongoOrderDetail>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof(MongoShipper)))
            {
                BsonClassMap.RegisterClassMap<MongoShipper>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof(MongoEntityChangeLog)))
            {
                BsonClassMap.RegisterClassMap<MongoEntityChangeLog>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }
        }
        catch (Exception ex)
        {
            // Log the exception to understand what's causing the issue
            System.Diagnostics.Debug.WriteLine($"BsonClassMap registration failed: {ex.Message}");
        }
    }

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

    /// <summary>
    /// Gets the underlying MongoDB database instance for advanced operations
    /// </summary>
    public IMongoDatabase Database { get; }
}