using Gamestore.Data.Interfaces;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.Json;

namespace Gamestore.Services.Services;

/// <summary>
/// Service for unified product operations across SQL and MongoDB databases
/// Implements E08 US4, US6, US7 requirements
/// </summary>
public class UnifiedProductService : IUnifiedProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnifiedProductService> _logger;
    private readonly IMongoCollection<BsonDocument> _mongoProductsCollection;

    public UnifiedProductService(
        IUnitOfWork unitOfWork,
        ILogger<UnifiedProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;

        // Bezpośrednie połączenie z MongoDB
        var client = new MongoClient("mongodb://localhost:27017");
        var database = client.GetDatabase("Northwind");
        _mongoProductsCollection = database.GetCollection<BsonDocument>("products");
    }

    /// <summary>
    /// Gets all products with duplicate management (E08 US6)
    /// SQL database has priority over MongoDB duplicates
    /// </summary>
    public async Task<IEnumerable<object>> GetAllProductsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all products from both databases");

            // 1. Pobierz products z SQL
            var sqlProducts = await GetSqlProductsAsync();

            // 2. Pobierz products z MongoDB
            var mongoProducts = await GetMongoProductsAsync();

            // 3. Implementuj duplicate management (E08 US6)
            var unifiedProducts = ApplyDuplicateManagement(sqlProducts, mongoProducts);

            _logger.LogInformation("Unified products: {SqlCount} from SQL, {MongoCount} from MongoDB",
                sqlProducts.Count(), mongoProducts.Count());

            return unifiedProducts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching unified products");
            throw;
        }
    }

    /// <summary>
    /// Gets product by ID from either database
    /// </summary>
    public async Task<object?> GetProductByIdAsync(string id)
    {
        try
        {
            // Sprawdź czy to GUID (SQL) czy int (MongoDB)
            if (Guid.TryParse(id, out var guid))
            {
                // Szukaj w SQL
                var game = await _unitOfWork.Games.GetByIdAsync(guid);
                if (game != null)
                {
                    return FormatSqlProduct(game);
                }
            }

            // Szukaj w MongoDB
            if (int.TryParse(id, out var productId))
            {
                var filter = Builders<BsonDocument>.Filter.Eq("ProductID", productId);
                var mongoProduct = await _mongoProductsCollection.Find(filter).FirstOrDefaultAsync();
                if (mongoProduct != null)
                {
                    return FormatMongoProduct(mongoProduct);
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by ID: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets product by key
    /// </summary>
    public async Task<object?> GetProductByKeyAsync(string key)
    {
        try
        {
            // Najpierw sprawdź SQL
            var game = await _unitOfWork.Games.GetKeyAsync(key);
            if (game != null)
            {
                return FormatSqlProduct(game);
            }

            // Potem sprawdź MongoDB
            var filter = Builders<BsonDocument>.Filter.Eq("GameKey", key);
            var mongoProduct = await _mongoProductsCollection.Find(filter).FirstOrDefaultAsync();
            if (mongoProduct != null)
            {
                return FormatMongoProduct(mongoProduct);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by key: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Creates product (always in SQL database)
    /// </summary>
    public async Task<object> CreateProductAsync(object productData)
    {
        try
        {
            _logger.LogInformation("Creating new product in SQL database");

            // Tutaj dodasz logikę tworzenia Game w SQL
            // Na razie return placeholder
            return new { message = "Product creation not yet implemented" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            throw;
        }
    }

    /// <summary>
    /// Updates product (E08 US7 logic)
    /// </summary>
    public async Task<object> UpdateProductAsync(string id, object productData)
    {
        try
        {
            _logger.LogInformation("Updating product: {Id}", id);

            // E08 US7: If editing MongoDB item, copy to SQL and update there
            // MongoDB data should not be affected by CRUD operations

            // Na razie return placeholder
            return new { message = "Product update not yet implemented" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Deletes product (only from SQL database)
    /// </summary>
    public async Task<bool> DeleteProductAsync(string id)
    {
        try
        {
            _logger.LogInformation("Deleting product from SQL: {Id}", id);

            if (!Guid.TryParse(id, out var guid))
            {
                return false; // Can only delete SQL products
            }

            var game = await _unitOfWork.Games.GetByIdAsync(guid);
            if (game == null)
            {
                return false;
            }

            await _unitOfWork.Games.DeleteAsync(guid);
            await _unitOfWork.CompleteAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Syncs unit-in-stock count between databases (E08 US5)
    /// </summary>
    public async Task<object> SyncStockCountAsync(string productId, int newStock)
    {
        try
        {
            _logger.LogInformation("Syncing stock count for product {ProductId} to {NewStock}", productId, newStock);

            // Update in SQL if it exists
            if (Guid.TryParse(productId, out var guid))
            {
                var game = await _unitOfWork.Games.GetByIdAsync(guid);
                if (game != null)
                {
                    var oldStock = game.UnitInStock;
                    game.UnitInStock = newStock;
                    await _unitOfWork.Games.UpdateAsync(game);
                    await _unitOfWork.CompleteAsync();

                    await LogEntityChangeAsync("StockSync", "Game", productId,
                        new { UnitInStock = oldStock },
                        new { UnitInStock = newStock });

                    return new { message = "Stock updated in SQL database", productId, newStock };
                }
            }

            // For MongoDB products, we don't update (read-only)
            return new { message = "MongoDB products are read-only", productId };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing stock for product {ProductId}", productId);
            throw;
        }
    }

    // Private helper methods...

    private async Task<IEnumerable<object>> GetSqlProductsAsync()
    {
        var games = await _unitOfWork.Games.GetAllAsync();
        return games.Select(FormatSqlProduct);
    }

    private async Task<IEnumerable<object>> GetMongoProductsAsync()
    {
        var documents = await _mongoProductsCollection.Find(new BsonDocument()).ToListAsync();
        return documents.Select(FormatMongoProduct);
    }

    private object FormatSqlProduct(dynamic game)
    {
        return new
        {
            id = game.Id.ToString(),
            name = game.Name,
            key = game.Key,
            price = game.Price,
            unitsInStock = game.UnitInStock,
            discontinued = game.Discontinued == 1,
            viewCount = game.ViewCount,
            source = "SQL"
        };
    }

    private object FormatMongoProduct(BsonDocument doc)
    {
        return new
        {
            id = doc.Contains("ProductID") ? doc["ProductID"].ToString() : doc["_id"].ToString(),
            name = doc.Contains("ProductName") ? doc["ProductName"].AsString : "N/A",
            key = doc.Contains("GameKey") ? doc["GameKey"].AsString : null,
            price = doc.Contains("UnitPrice") ? (double?)doc["UnitPrice"].ToDouble() : null,
            unitsInStock = doc.Contains("UnitsInStock") ? (int?)doc["UnitsInStock"].ToInt32() : null,
            discontinued = doc.Contains("Discontinued") ? doc["Discontinued"].ToBoolean() : false,
            viewCount = doc.Contains("ViewCount") ? doc["ViewCount"].ToInt32() : 0,
            source = "MongoDB"
        };
    }

    private async Task LogEntityChangeAsync(string actionName, string entityType, string entityId, object? oldVersion, object? newVersion)
    {
        try
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("Northwind");
            var logsCollection = database.GetCollection<BsonDocument>("entitychangelogs");

            var logEntry = new BsonDocument
        {
            { "Timestamp", DateTime.UtcNow },
            { "ActionName", actionName },
            { "EntityType", entityType },
            { "EntityId", entityId },
            { "OldVersion", oldVersion != null ? BsonDocument.Parse(JsonSerializer.Serialize(oldVersion)) : BsonNull.Value },
            { "NewVersion", newVersion != null ? BsonDocument.Parse(JsonSerializer.Serialize(newVersion)) : BsonNull.Value },
            { "DatabaseSource", "Unified" }
        };

            await logsCollection.InsertOneAsync(logEntry);
            _logger.LogInformation("Entity change logged: {ActionName} for {EntityType} {EntityId}", actionName, entityType, entityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log entity change");
        }
    }

    private IEnumerable<object> ApplyDuplicateManagement(IEnumerable<object> sqlProducts, IEnumerable<object> mongoProducts)
    {
        var result = new List<object>();

        // Dodaj wszystkie SQL products (mają priorytet)
        result.AddRange(sqlProducts);

        // Dodaj MongoDB products, ale sprawdź duplikaty po nazwie
        var sqlProductNames = sqlProducts.Cast<dynamic>().Select(p => (string)p.name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var uniqueMongoProducts = mongoProducts.Cast<dynamic>()
            .Where(mp => !sqlProductNames.Contains((string)mp.name))
            .GroupBy(mp => (string)mp.name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First()) // First found item for MongoDB duplicates
            .Cast<object>();

        result.AddRange(uniqueMongoProducts);

        return result;
    }
}