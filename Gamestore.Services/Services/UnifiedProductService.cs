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

            // 3. Apply duplicate management (E08 US6)
            var unifiedProducts = ApplyDuplicateManagement(sqlProducts, mongoProducts);

            _logger.LogInformation("Successfully fetched {Count} unified products", unifiedProducts.Count);
            return unifiedProducts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all products");
            throw;
        }
    }

    /// <summary>
    /// Gets product by ID from SQL first, then MongoDB if not found
    /// </summary>
    public async Task<object?> GetProductByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("GetProductByIdAsync called with null or empty ID");
                return null;
            }

            _logger.LogInformation("Getting product by ID: {Id}", id);

            // Sprawdź SQL (Guid)
            if (Guid.TryParse(id, out var guid))
            {
                var game = await _unitOfWork.Games.GetByIdAsync(guid);
                if (game != null)
                {
                    _logger.LogInformation("Found product in SQL database with ID: {Id}", id);
                    return FormatSqlProduct(game);
                }
            }

            // Sprawdź MongoDB (int ProductId)
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

            if (gameData == null)
            {
                throw new ArgumentException("Invalid product data");
            }

            // Use TryGetValue instead of ContainsKey + indexer (CA1854)
            var game = new Game
            {
                Name = gameData.TryGetValue("name", out var name) ? name?.ToString() ?? "New Product" : "New Product",
                Key = gameData.TryGetValue("key", out var key) ? key?.ToString() ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString(),
                Price = gameData.TryGetValue("price", out var price) ? (double)Convert.ToDecimal(price) : 0.0,
                UnitInStock = gameData.TryGetValue("unitsInStock", out var stock) ? Convert.ToInt32(stock) : 0,
                Discontinued = gameData.TryGetValue("discontinued", out var disc) ? Convert.ToBoolean(disc) ? 1 : 0 : 0,
                ViewCount = 0,
                Description = gameData.TryGetValue("description", out var desc) ? desc?.ToString() ?? string.Empty : string.Empty,
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

            if (productDict == null)
            {
                throw new InvalidOperationException("Failed to deserialize product data");
            }

            // Check if this is a MongoDB product
            if (productDict.TryGetValue("source", out var source) && source?.ToString() == "MongoDB")
            {
                // E08 US7: Copy MongoDB product to SQL first, then update
                var sqlProduct = await CopyMongoProductToSql(id);
                return await UpdateSqlProduct(sqlProduct.Id.ToString(), productData);
            }
            else
            {
                // Update existing SQL product
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
    /// Syncs stock count between databases (E08 US5)
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
                        new { stock = oldStock }, new { stock = newStock });

                    _logger.LogInformation("Successfully synced stock for SQL product {ProductId} from {OldStock} to {NewStock}",
                        productId, oldStock, newStock);

                    return FormatSqlProduct(game);
                }
            }

            // Try MongoDB if not found in SQL
            if (int.TryParse(productId, out var mongoProductId))
            {
                var mongoProducts = await _mongoProductRepository.GetAsync(p => p.ProductId == mongoProductId);
                var mongoProduct = mongoProducts.FirstOrDefault();

                if (mongoProduct != null)
                {
                    // MongoDB is read-only per E08 US7, so copy to SQL first
                    _logger.LogInformation("MongoDB product found, copying to SQL for stock sync");
                    var copiedGame = await CopyMongoProductToSql(productId);
                    copiedGame.UnitInStock = newStock;

                    await _unitOfWork.Games.UpdateAsync(copiedGame);
                    await _unitOfWork.CompleteAsync();

                    await LogEntityChangeAsync("StockSync", "Game", copiedGame.Id.ToString(),
                        new { stock = 0 }, new { stock = newStock });

                    _logger.LogInformation("Successfully copied MongoDB product and synced stock to {NewStock}", newStock);
                    return FormatSqlProduct(copiedGame);
                }
            }

            throw new KeyNotFoundException($"Product with ID {productId} not found in any database");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing stock for product {ProductId}", productId);
            throw;
        }
    }

    /// <summary>
    /// Deletes product (only from SQL database, MongoDB is read-only per E08 US7)
    /// </summary>
    public async Task<bool> DeleteProductAsync(string id)
    {
        try
        {
            _logger.LogInformation("Deleting product: {Id}", id);

            if (!Guid.TryParse(id, out var guid))
            {
                throw new ArgumentException($"Invalid product ID for deletion: {id}");
            }

            var game = await _unitOfWork.Games.GetByIdAsync(guid);
            if (game == null)
            {
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

    private static List<object> ApplyDuplicateManagement(IEnumerable<object> sqlProducts, IEnumerable<object> mongoProducts)
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

    private static string GetProductIdentifier(object product)
    {
        var productDict = product as Dictionary<string, object> ??
                         JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(product));

        // Use name as identifier for duplicate detection
        return productDict?["name"]?.ToString()?.ToLower() ?? string.Empty;
    }

    private static object FormatSqlProduct(Game game)
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
            description = game.Description,
            source = "SQL"
        };
    }

    private static object FormatMongoProduct(MongoProduct product)
    {
        return new
        {
            id = product.ProductId.ToString(),
            name = product.ProductName,
            key = product.GameKey,
            price = (double)(product.UnitPrice ?? 0),
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

        if (updateData == null)
        {
            throw new ArgumentException("Invalid update data");
        }

        // Update fields using TryGetValue (CA1854)
        if (updateData.TryGetValue("name", out var name))
            game.Name = name?.ToString() ?? game.Name;
        if (updateData.TryGetValue("price", out var price))
            game.Price = (double)Convert.ToDecimal(price);
        if (updateData.TryGetValue("unitsInStock", out var stock))
            game.UnitInStock = Convert.ToInt32(stock);
        if (updateData.TryGetValue("discontinued", out var disc))
            game.Discontinued = Convert.ToBoolean(disc) ? 1 : 0;
        if (updateData.TryGetValue("description", out var desc))
            game.Description = desc?.ToString() ?? game.Description;

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