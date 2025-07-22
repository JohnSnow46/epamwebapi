using Gamestore.Data.Interfaces;
using Gamestore.Entities.MongoDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gamestore.Data.MongoDB;

/// <summary>
/// Extension methods for registering MongoDB services in DI container
/// </summary>
public static class MongoServiceExtensions
{
    /// <summary>
    /// Registers MongoDB context and repositories in the DI container
    /// </summary>
    public static IServiceCollection AddMongoDbServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        // Check if MongoDB is enabled
        var mongoSettings = configuration.GetSection("MongoDB");

        // Register MongoDB context
        services.AddScoped<MongoDbContext>();

        // Register MongoDB repositories
        services.AddScoped<IMongoRepository<MongoProduct>>(provider =>
        {
            var context = provider.GetRequiredService<MongoDbContext>();
            return new MongoRepository<MongoProduct>(context.Products);
        });

        services.AddScoped<IMongoRepository<MongoCategory>>(provider =>
        {
            var context = provider.GetRequiredService<MongoDbContext>();
            return new MongoRepository<MongoCategory>(context.Categories);
        });

        services.AddScoped<IMongoRepository<MongoSupplier>>(provider =>
        {
            var context = provider.GetRequiredService<MongoDbContext>();
            return new MongoRepository<MongoSupplier>(context.Suppliers);
        });

        services.AddScoped<IMongoRepository<MongoOrder>>(provider =>
        {
            var context = provider.GetRequiredService<MongoDbContext>();
            return new MongoRepository<MongoOrder>(context.Orders);
        });

        services.AddScoped<IMongoRepository<MongoOrderDetail>>(provider =>
        {
            var context = provider.GetRequiredService<MongoDbContext>();
            return new MongoRepository<MongoOrderDetail>(context.OrderDetails);
        });

        services.AddScoped<IMongoRepository<MongoShipper>>(provider =>
        {
            var context = provider.GetRequiredService<MongoDbContext>();
            return new MongoRepository<MongoShipper>(context.Shippers);
        });

        services.AddScoped<IMongoRepository<MongoEntityChangeLog>>(provider =>
        {
            var context = provider.GetRequiredService<MongoDbContext>();
            return new MongoRepository<MongoEntityChangeLog>(context.EntityChangeLogs);
        });

        // Register specific repositories
        services.AddScoped<IMongoProductRepository>(provider =>
        {
            var context = provider.GetRequiredService<MongoDbContext>();
            return new MongoProductRepository(context.Products);
        });

        return services;
    }
}