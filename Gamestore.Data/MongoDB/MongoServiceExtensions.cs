using Gamestore.Data.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gamestore.Data.MongoDB;

/// <summary>
/// Extension methods for registering MongoDB services in DI container
/// Updated to use BsonDocument approach to avoid BsonClassMap issues
/// </summary>
public static class MongoServiceExtensions
{
    /// <summary>
    /// Registers MongoDB context and repositories in the DI container
    /// </summary>
    public static IServiceCollection AddMongoDbServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        // Register MongoDB context
        services.AddScoped<MongoDbContext>();

        // Register only the specific product repository that works with BsonDocument
        services.AddScoped<IMongoProductRepository, MongoProductRepository>();

        // TODO: Implement other repositories using BsonDocument approach if needed
        // For now, only MongoProductRepository is implemented

        return services;
    }
}