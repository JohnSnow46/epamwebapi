using System.Text.Json;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Gamestore.Entities.MongoDB;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services;

/// <summary>
/// Service for unified product operations across SQL and MongoDB databases
/// Implements E08 US4, US6, US7 requirements
/// </summary>
public class UnifiedProductService : IUnifiedProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMongoProductRepository _mongoProductRepository;
    private readonly ILogger<UnifiedProductService> _logger;

    public UnifiedProductService(
        IUnitOfWork unitOfWork,
        IMongoProductRepository mongoProductRepository,
        ILogger<UnifiedProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _mongoProductRepository = mongoProductRepository;
        _logger = logger;
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
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("GetProductByIdAsync called with null or empty id");
                return null;
            }

            _logger.LogInformation("Getting product by ID: {Id}", id);

            // Sprawdź czy to GUID (SQL) czy int (MongoDB)
            if (Guid.TryParse(id, out var guid))
            {
                // Szukaj w SQL
                var game = await _unitOfWork.Games.GetByIdAsync(guid);
                if (game != null)
                {
                    _logger.LogInformation("Found product in SQL database with ID: {Id}", id);
                    return FormatSqlProduct(game);
                }
            }

            // Szukaj w MongoDB
            if (int.TryParse(id, out var productId))
            {
                var mongoProducts = await _mongoProductRepository.GetAsync(p => p.ProductId == productId);
                var mongoProduct = mongoProducts.FirstOrDefault();
                if (mongoProduct != null)
                {
                    _logger.LogInformation("Found product in MongoDB with ID: {Id}", id);
                    return FormatMongoProduct(mongoProduct);
                }
            }

            _logger.LogInformation("Product not found in any database with ID: {Id}", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by ID: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets product by key from SQL database first, then MongoDB if not found
    /// If key not found, tries to search by ProductName in MongoDB as fallback
    /// </summary>
    /// <param name="key">Product key to search for</param>
    /// <returns>Product data or null if not found</returns>
    public async Task<object?> GetProductByKeyAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("GetProductByKeyAsync called with null or empty key");
                return null;
            }

            _logger.LogInformation("Getting product by key: {Key}", key);

            // Najpierw sprawdź SQL
            var game = await _unitOfWork.Games.GetKeyAsync(key);
            if (game != null)
            {
                _logger.LogInformation("Found product in SQL database with key: {Key}", key);
                return FormatSqlProduct(game);
            }

            // Sprawdź MongoDB po GameKey
            var mongoProduct = await _mongoProductRepository.GetByGameKeyAsync(key);
            if (mongoProduct != null)
            {
                _logger.LogInformation("Found product in MongoDB with GameKey: {Key}", key);
                return FormatMongoProduct(mongoProduct);
            }

            // Fallback: sprawdź czy key to ProductName w MongoDB (dla produktów bez GameKey)
            var mongoProducts = await _mongoProductRepository.GetAsync(p => p.ProductName.ToLower() == key.ToLower());
            var productByName = mongoProducts.FirstOrDefault();

            if (productByName != null)
            {
                _logger.LogInformation("Found product in MongoDB with ProductName: {Key}", key);
                return FormatMongoProduct(productByName);
            }

            // Ostatni fallback: sprawdź czy key to ProductID (dla produktów identyfikowanych po ID)
            if (int.TryParse(key, out int productId))
            {
                var mongoProductById = await _mongoProductRepository.GetAsync(p => p.ProductId == productId);
                var productById = mongoProductById.FirstOrDefault();

                if (productById != null)
                {
                    _logger.LogInformation("Found product in MongoDB with ProductID: {Key}", key);
                    return FormatMongoProduct(productById);
                }
            }

            _logger.LogInformation("Product not found in any database with key: {Key}", key);
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

            // Convert productData to Game entity
            var json = JsonSerializer.Serialize(productData);
            var gameData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            var game = new Game
            {
                Name = gameData.ContainsKey("name") ? gameData["name"].ToString() : "New Product",
                Key = gameData.ContainsKey("key") ? gameData["key"].ToString() : Guid.NewGuid().ToString(),
                Price = (double)(gameData.ContainsKey("price") ? Convert.ToDecimal(gameData["price"]) : 0m),
                UnitInStock = gameData.ContainsKey("unitsInStock") ? Convert.ToInt32(gameData["unitsInStock"]) : 0,
                Discontinued = gameData.ContainsKey("discontinued") ? Convert.ToBoolean(gameData["discontinued"]) ? 1 : 0 : 0,
                ViewCount = 0,
                Description = gameData.ContainsKey("description") ? gameData["description"].ToString() : string.Empty,
                PublisherId = null // Set based on your business logic
            };

            await _unitOfWork.Games.AddAsync(game);
            await _unitOfWork.CompleteAsync();

            await LogEntityChangeAsync("Create", "Game", game.Id.ToString(), null, FormatSqlProduct(game));

            _logger.LogInformation("Successfully created product with ID: {Id}", game.Id);
            return FormatSqlProduct(game);
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
            var existingProduct = await GetProductByIdAsync(id);
            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found");
            }

            var productDict = existingProduct as Dictionary<string, object> ??
                             JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(existingProduct));

            // Check if it's MongoDB product
            if (productDict["source"].ToString() == "MongoDB")
            {
                _logger.LogInformation("MongoDB product detected, copying to SQL before update");

                // Copy to SQL first
                var newGame = await CopyMongoProductToSql(id);

                // Then update the SQL version
                await UpdateSqlProduct(newGame.Id.ToString(), productData);

                return FormatSqlProduct(newGame);
            }
            else
            {
                // Update SQL product directly
                return await UpdateSqlProduct(id, productData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Syncs stock count (E08 US5)
    /// </summary>
    public async Task<object> SyncStockCountAsync(string productId, int newStock)
    {
        try
        {
            _logger.LogInformation("Syncing stock for product {ProductId} to {NewStock}", productId, newStock);

            // Try to find in SQL first
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

    /// <summary>
    /// Deletes a product (only from SQL database)
    /// </summary>
    public async Task<bool> DeleteProductAsync(string id)
    {
        try
        {
            _logger.LogInformation("Deleting product: {Id}", id);

            if (!Guid.TryParse(id, out var guid))
            {
                _logger.LogWarning("Invalid GUID format for product deletion: {Id}", id);
                return false;
            }

            var game = await _unitOfWork.Games.GetByIdAsync(guid);
            if (game == null)
            {
                _logger.LogWarning("Product not found for deletion: {Id}", id);
                return false;
            }

            var oldProduct = FormatSqlProduct(game);
            await _unitOfWork.Games.DeleteAsync(guid);
            await _unitOfWork.CompleteAsync();

            await LogEntityChangeAsync("Delete", "Game", id, oldProduct, null);

            _logger.LogInformation("Successfully deleted product: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product: {Id}", id);
            throw;
        }
    }

    // Private helper methods

    private async Task<IEnumerable<object>> GetSqlProductsAsync()
    {
        var games = await _unitOfWork.Games.GetAllAsync();
        return games.Select(FormatSqlProduct);
    }

    private async Task<IEnumerable<object>> GetMongoProductsAsync()
    {
        // Use GetAsync instead of GetAllAsync since it doesn't exist
        var mongoProducts = await _mongoProductRepository.GetAsync(_ => true);
        return mongoProducts.Select(FormatMongoProduct);
    }

    private IEnumerable<object> ApplyDuplicateManagement(IEnumerable<object> sqlProducts, IEnumerable<object> mongoProducts)
    {
        // E08 US6: SQL database has priority over MongoDB duplicates
        var result = new List<object>();
        var sqlProductsDict = sqlProducts.ToDictionary(p => GetProductIdentifier(p), p => p);

        // Add all SQL products first
        result.AddRange(sqlProducts);

        // Add MongoDB products that don't exist in SQL
        foreach (var mongoProduct in mongoProducts)
        {
            var identifier = GetProductIdentifier(mongoProduct);
            if (!sqlProductsDict.ContainsKey(identifier))
            {
                result.Add(mongoProduct);
            }
        }

        return result;
    }

    private string GetProductIdentifier(object product)
    {
        var productDict = product as Dictionary<string, object> ??
                         JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(product));

        // Use name as identifier for duplicate detection
        return productDict["name"]?.ToString()?.ToLower() ?? string.Empty;
    }

    private object FormatSqlProduct(Game game)
    {
        return new
        {
            id = game.Id.ToString(),
            name = game.Name,
            key = game.Key,
            price = (double)game.Price,
            unitsInStock = game.UnitInStock,
            discontinued = game.Discontinued == 1,
            viewCount = game.ViewCount,
            description = game.Description,
            source = "SQL"
        };
    }

    private object FormatMongoProduct(MongoProduct product)
    {
        return new
        {
            id = product.ProductId.ToString(),
            name = product.ProductName,
            key = product.GameKey,
            price = (double?)(product.UnitPrice),
            unitsInStock = product.UnitsInStock,
            discontinued = product.Discontinued,
            viewCount = product.ViewCount,
            description = product.QuantityPerUnit,
            source = "MongoDB"
        };
    }

    private async Task<Game> CopyMongoProductToSql(string mongoProductId)
    {
        if (!int.TryParse(mongoProductId, out var productId))
        {
            throw new ArgumentException($"Invalid MongoDB product ID: {mongoProductId}");
        }

        var mongoProducts = await _mongoProductRepository.GetAsync(p => p.ProductId == productId);
        var mongoProduct = mongoProducts.FirstOrDefault();

        if (mongoProduct == null)
        {
            throw new KeyNotFoundException($"MongoDB product with ID {mongoProductId} not found");
        }

        var game = new Game
        {
            Name = mongoProduct.ProductName,
            Key = mongoProduct.GameKey ?? $"mongo-{mongoProduct.ProductId}",
            Price = (double)(mongoProduct.UnitPrice ?? 0),
            UnitInStock = mongoProduct.UnitsInStock ?? 0,
            Discontinued = mongoProduct.Discontinued ? 1 : 0,
            ViewCount = mongoProduct.ViewCount,
            Description = mongoProduct.QuantityPerUnit ?? string.Empty,
            PublisherId = null // Map based on SupplierID if needed
        };

        await _unitOfWork.Games.AddAsync(game);
        await _unitOfWork.CompleteAsync();

        await LogEntityChangeAsync("CopyFromMongo", "Game", game.Id.ToString(),
            FormatMongoProduct(mongoProduct), FormatSqlProduct(game));

        return game;
    }

    private async Task<object> UpdateSqlProduct(string id, object productData)
    {
        if (!Guid.TryParse(id, out var guid))
        {
            throw new ArgumentException($"Invalid SQL product ID: {id}");
        }

        var game = await _unitOfWork.Games.GetByIdAsync(guid);
        if (game == null)
        {
            throw new KeyNotFoundException($"SQL product with ID {id} not found");
        }

        var oldProduct = FormatSqlProduct(game);
        var json = JsonSerializer.Serialize(productData);
        var updateData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

        // Update fields
        if (updateData.ContainsKey("name"))
            game.Name = updateData["name"].ToString();
        if (updateData.ContainsKey("price"))
            game.Price = (double)Convert.ToDecimal(updateData["price"]);
        if (updateData.ContainsKey("unitsInStock"))
            game.UnitInStock = Convert.ToInt32(updateData["unitsInStock"]);
        if (updateData.ContainsKey("discontinued"))
            game.Discontinued = Convert.ToBoolean(updateData["discontinued"]) ? 1 : 0;
        if (updateData.ContainsKey("description"))
            game.Description = updateData["description"].ToString();

        await _unitOfWork.Games.UpdateAsync(game);
        await _unitOfWork.CompleteAsync();

        var newProduct = FormatSqlProduct(game);
        await LogEntityChangeAsync("Update", "Game", id, oldProduct, newProduct);

        return newProduct;
    }

    private async Task LogEntityChangeAsync(string actionName, string entityType, string entityId, object? oldVersion, object? newVersion)
    {
        try
        {
            // This should log to MongoDB EntityChangeLogs collection
            // Implementation depends on your logging infrastructure
            _logger.LogInformation("Entity change logged: {Action} {EntityType} {EntityId}",
                actionName, entityType, entityId);

            // Add actual MongoDB logging here if needed
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log entity change");
            // Don't throw - logging failure shouldn't break the main operation
        }
    }
}