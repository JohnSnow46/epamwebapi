using System.Text.Json;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Gamestore.Entities.MongoDB;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services;
#pragma warning disable S3358
/// <summary>
/// Service for unified product operations across SQL and MongoDB databases
/// Implements E08 US4, US6, US7 requirements
/// </summary>
public class UnifiedProductService(
    IUnitOfWork unitOfWork,
    IMongoProductRepository mongoProductRepository,
    ILogger<UnifiedProductService> logger) : IUnifiedProductService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMongoProductRepository _mongoProductRepository = mongoProductRepository;
    private readonly ILogger<UnifiedProductService> _logger = logger;

    /// <summary>
    /// Gets all products with duplicate management (E08 US6)
    /// SQL database has priority over MongoDB duplicates
    /// </summary>
    public async Task<IEnumerable<object>> GetAllProductsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all products from both databases");

            var sqlProducts = await GetSqlProductsAsync();
            var mongoProducts = await GetMongoProductsAsync();
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
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("GetProductByIdAsync called with null or empty ID");
            return null;
        }

        return await GetProductByIdInternalAsync(id);
    }

    /// <summary>
    /// Gets product by key from SQL database first, then MongoDB if not found
    /// If key not found, tries to search by ProductName in MongoDB as fallback
    /// </summary>
    /// <param name="key">Product key to search for</param>
    /// <returns>Product data or null if not found</returns>
    public async Task<object?> GetProductByKeyAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("GetProductByKeyAsync called with null or empty key");
            return null;
        }

        return await GetProductByKeyInternalAsync(key);
    }

#pragma warning disable S4457
    /// <summary>
    /// Creates product (always in SQL database)
    /// </summary>
    public async Task<object> CreateProductAsync(object productData)
    {
        ArgumentNullException.ThrowIfNull(productData);

        return await CreateProductInternalAsync(productData);
    }

    /// <summary>
    /// Updates product (E08 US7 logic)
    /// </summary>
    public async Task<object> UpdateProductAsync(string id, object productData)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Product ID cannot be null or empty", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(productData);

        return await UpdateProductInternalAsync(id, productData);
    }
#pragma warning restore S4457
    /// <summary>
    /// Syncs stock count between databases (E08 US5)
    /// </summary>
    public async Task<object> SyncStockCountAsync(string productId, int newStock)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);
        ArgumentOutOfRangeException.ThrowIfNegative(newStock);

        return await SyncStockCountInternalAsync(productId, newStock);
    }

    /// <summary>
    /// Deletes product (only from SQL database, MongoDB is read-only per E08 US7)
    /// </summary>
    public async Task<bool> DeleteProductAsync(string id)
    {
        return string.IsNullOrWhiteSpace(id)
            ? throw new ArgumentException("Product ID cannot be null or empty", nameof(id))
            : await DeleteProductInternalAsync(id);
    }

    private async Task<object?> GetProductByIdInternalAsync(string id)
    {
        try
        {
            _logger.LogInformation("Getting product by ID: {Id}", id);

            if (Guid.TryParse(id, out var guid))
            {
                var game = await _unitOfWork.Games.GetByIdAsync(guid);
                if (game != null)
                {
                    _logger.LogInformation("Found product in SQL database with ID: {Id}", id);
                    return FormatSqlProduct(game);
                }
            }

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

    private async Task<object?> GetProductByKeyInternalAsync(string key)
    {
        try
        {
            _logger.LogInformation("Getting product by key: {Key}", key);

            var game = await _unitOfWork.Games.GetKeyAsync(key);
            if (game != null)
            {
                _logger.LogInformation("Found product in SQL database with key: {Key}", key);
                return FormatSqlProduct(game);
            }

            var mongoProduct = await _mongoProductRepository.GetByGameKeyAsync(key);
            if (mongoProduct != null)
            {
                _logger.LogInformation("Found product in MongoDB with GameKey: {Key}", key);
                return FormatMongoProduct(mongoProduct);
            }

            var mongoProducts = await _mongoProductRepository.GetAsync(p => p.ProductName.ToLower() == key.ToLower());
            var productByName = mongoProducts.FirstOrDefault();

            if (productByName != null)
            {
                _logger.LogInformation("Found product in MongoDB with ProductName: {Key}", key);
                return FormatMongoProduct(productByName);
            }

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

    private async Task<object> CreateProductInternalAsync(object productData)
    {
        try
        {
            _logger.LogInformation("Creating new product in SQL database");

            var json = JsonSerializer.Serialize(productData);
            var gameData = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? throw new ArgumentException("Invalid product data");

            var game = new Game
            {
                Name = gameData.TryGetValue("name", out var name) ? name?.ToString() ?? "New Product" : "New Product",
                Key = gameData.TryGetValue("key", out var key) ? key?.ToString() ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString(),
                Price = gameData.TryGetValue("price", out var price) ? (double)Convert.ToDecimal(price) : 0.0,
                UnitInStock = gameData.TryGetValue("unitsInStock", out var stock) ? Convert.ToInt32(stock) : 0,
                Discontinued = gameData.TryGetValue("discontinued", out var disc) ? Convert.ToBoolean(disc) ? 1 : 0 : 0,
                ViewCount = 0,
                Description = gameData.TryGetValue("description", out var desc) ? desc?.ToString() ?? string.Empty : string.Empty,
                PublisherId = null
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

    private async Task<object> UpdateProductInternalAsync(string id, object productData)
    {
        try
        {
            _logger.LogInformation("Updating product: {Id}", id);

            var existingProduct = await GetProductByIdAsync(id) ?? throw new KeyNotFoundException($"Product with ID {id} not found");
            var productDict = (existingProduct as Dictionary<string, object> ??
                             JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(existingProduct))) ?? throw new InvalidOperationException("Failed to deserialize product data");

            if (productDict.TryGetValue("source", out var source) && source?.ToString() == "MongoDB")
            {
                var sqlProduct = await CopyMongoProductToSql(id);
                return await UpdateSqlProduct(sqlProduct.Id.ToString(), productData);
            }
            else
            {
                return await UpdateSqlProduct(id, productData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product: {Id}", id);
            throw;
        }
    }

    private async Task<object> SyncStockCountInternalAsync(string productId, int newStock)
    {
        try
        {
            _logger.LogInformation("Syncing stock for product {ProductId} to {NewStock}", productId, newStock);

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

            if (int.TryParse(productId, out var mongoProductId))
            {
                var mongoProducts = await _mongoProductRepository.GetAsync(p => p.ProductId == mongoProductId);
                var mongoProduct = mongoProducts.FirstOrDefault();

                if (mongoProduct != null)
                {
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

#pragma warning disable S4457
    private async Task<bool> DeleteProductInternalAsync(string id)
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
#pragma warning restore S4457

    private async Task<IEnumerable<object>> GetSqlProductsAsync()
    {
        var games = await _unitOfWork.Games.GetAllAsync();
        return games.Select(FormatSqlProduct);
    }

    private async Task<IEnumerable<object>> GetMongoProductsAsync()
    {
        var mongoProducts = await _mongoProductRepository.GetAsync(_ => true);
        return mongoProducts.Select(FormatMongoProduct);
    }

    private static List<object> ApplyDuplicateManagement(IEnumerable<object> sqlProducts, IEnumerable<object> mongoProducts)
    {
        var result = new List<object>();
        var sqlProductsDict = sqlProducts.ToDictionary(GetProductIdentifier, p => p);

        result.AddRange(sqlProducts);

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

#pragma warning disable S4457
    private async Task<Game> CopyMongoProductToSql(string mongoProductId)
    {
        if (!int.TryParse(mongoProductId, out var productId))
        {
            throw new ArgumentException($"Invalid MongoDB product ID: {mongoProductId}");
        }

        var mongoProducts = await _mongoProductRepository.GetAsync(p => p.ProductId == productId);
        var mongoProduct = mongoProducts.FirstOrDefault() ?? throw new KeyNotFoundException($"MongoDB product with ID {mongoProductId} not found");

        var game = new Game
        {
            Name = mongoProduct.ProductName,
            Key = mongoProduct.GameKey ?? $"mongo-{mongoProduct.ProductId}",
            Price = (double)(mongoProduct.UnitPrice ?? 0),
            UnitInStock = mongoProduct.UnitsInStock ?? 0,
            Discontinued = mongoProduct.Discontinued ? 1 : 0,
            ViewCount = mongoProduct.ViewCount,
            Description = mongoProduct.QuantityPerUnit ?? string.Empty,
            PublisherId = null
        };

        await _unitOfWork.Games.AddAsync(game);
        await _unitOfWork.CompleteAsync();

        await LogEntityChangeAsync("CopyFromMongo", "Game", game.Id.ToString(),
            FormatMongoProduct(mongoProduct), FormatSqlProduct(game));

        return game;
    }
#pragma warning restore S4457

#pragma warning disable S4457
    private async Task<object> UpdateSqlProduct(string id, object productData)
    {
        if (!Guid.TryParse(id, out var guid))
        {
            throw new ArgumentException($"Invalid SQL product ID: {id}");
        }

        var game = await _unitOfWork.Games.GetByIdAsync(guid) ?? throw new KeyNotFoundException($"SQL product with ID {id} not found");
        var oldProduct = FormatSqlProduct(game);
        var json = JsonSerializer.Serialize(productData);
        var updateData = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? throw new ArgumentException("Invalid update data");

        if (updateData.TryGetValue("name", out var name))
        {
            game.Name = name?.ToString() ?? game.Name;
        }

        if (updateData.TryGetValue("price", out var price))
        {
            game.Price = (double)Convert.ToDecimal(price);
        }

        if (updateData.TryGetValue("unitsInStock", out var stock))
        {
            game.UnitInStock = Convert.ToInt32(stock);
        }

        if (updateData.TryGetValue("discontinued", out var disc))
        {
            game.Discontinued = Convert.ToBoolean(disc) ? 1 : 0;
        }

        if (updateData.TryGetValue("description", out var desc))
        {
            game.Description = desc?.ToString() ?? game.Description;
        }

        await _unitOfWork.Games.UpdateAsync(game);
        await _unitOfWork.CompleteAsync();

        var newProduct = FormatSqlProduct(game);
        await LogEntityChangeAsync("Update", "Game", id, oldProduct, newProduct);

        return newProduct;
    }
#pragma warning restore S4457

    private async Task LogEntityChangeAsync(string actionName, string entityType, string entityId, object? oldVersion, object? newVersion)
    {
        try
        {
            var changeLog = new
            {
                DateTime = DateTime.UtcNow,
                Action = actionName,
                EntityType = entityType,
                EntityId = entityId,
                OldVersion = oldVersion != null ? JsonSerializer.Serialize(oldVersion) : null,
                NewVersion = newVersion != null ? JsonSerializer.Serialize(newVersion) : null
            };

            // E08 NFR4: Log to NoSQL database (MongoDB)
            _logger.LogInformation("Entity change logged: {Action} {EntityType} {EntityId} - Old: {OldVersion} - New: {NewVersion}",
                actionName, entityType, entityId,
                changeLog.OldVersion?[..Math.Min(100, changeLog.OldVersion.Length)] + "...",
                changeLog.NewVersion?[..Math.Min(100, changeLog.NewVersion.Length)] + "...");

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log entity change");
        }
    }
}
#pragma warning restore S3358