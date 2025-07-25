using Gamestore.Data.Interfaces;
using Gamestore.Entities.MongoDB;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Linq.Expressions;

namespace Gamestore.Data.MongoDB;

/// <summary>
/// MongoDB Product Repository using BsonDocument to avoid BsonClassMap issues
/// Converts BsonDocument to MongoProduct manually
/// </summary>
public class MongoProductRepository(MongoDbContext context) : IMongoProductRepository
{
    private readonly IMongoCollection<BsonDocument> _collection = context.ProductsRaw;

    // IMongoProductRepository specific methods
    public async Task<MongoProduct?> GetByGameKeyAsync(string gameKey)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("GameKey", gameKey);
        var doc = await _collection.Find(filter).FirstOrDefaultAsync();
        return doc != null ? ConvertToMongoProduct(doc) : null;
    }

    public async Task<IEnumerable<MongoProduct>> GetByCategoryIdAsync(int categoryId)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("CategoryID", categoryId);
        var docs = await _collection.Find(filter).ToListAsync();
        return docs.Select(ConvertToMongoProduct);
    }

    public async Task<IEnumerable<MongoProduct>> GetBySupplierIdAsync(int supplierId)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("SupplierID", supplierId);
        var docs = await _collection.Find(filter).ToListAsync();
        return docs.Select(ConvertToMongoProduct);
    }

    public async Task<bool> UpdateGameKeyAsync(int productId, string gameKey)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("ProductID", productId);
        var update = Builders<BsonDocument>.Update.Set("GameKey", gameKey);
        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> IncrementViewCountAsync(int productId)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("ProductID", productId);
        var update = Builders<BsonDocument>.Update.Inc("ViewCount", 1);
        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<IEnumerable<MongoProduct>> GetAvailableProductsAsync()
    {
        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Eq("Discontinued", false),
            Builders<BsonDocument>.Filter.Gt("UnitsInStock", 0)
        );
        var docs = await _collection.Find(filter).ToListAsync();
        return docs.Select(ConvertToMongoProduct);
    }

    // IMongoRepository<MongoProduct> interface implementation
    public IQueryable<MongoProduct> AsQueryable()
    {
        // Not ideal but works for compatibility
        var docs = _collection.Find(new BsonDocument()).ToList();
        return docs.Select(ConvertToMongoProduct).AsQueryable();
    }

    public async Task<MongoProduct?> GetByIdAsync(object id)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
        var doc = await _collection.Find(filter).FirstOrDefaultAsync();
        return doc != null ? ConvertToMongoProduct(doc) : null;
    }

    public async Task<IEnumerable<MongoProduct>> GetAsync(Expression<Func<MongoProduct, bool>> filter)
    {
        // Get all and filter in memory (not ideal for large datasets)
        var docs = await _collection.Find(new BsonDocument()).ToListAsync();
        var products = docs.Select(ConvertToMongoProduct);
        return products.Where(filter.Compile());
    }

    public async Task<MongoProduct?> GetFirstOrDefaultAsync(Expression<Func<MongoProduct, bool>> filter)
    {
        var results = await GetAsync(filter);
        return results.FirstOrDefault();
    }

    public Task<bool> UpdateAsync(FilterDefinition<MongoProduct> filter, UpdateDefinition<MongoProduct> update)
    {
        throw new NotImplementedException("Use UpdateGameKeyAsync or IncrementViewCountAsync instead");
    }

    public Task<bool> UpdateManyAsync(FilterDefinition<MongoProduct> filter, UpdateDefinition<MongoProduct> update)
    {
        throw new NotImplementedException("Use specific update methods instead");
    }

    public async Task InsertOneAsync(MongoProduct entity)
    {
        var doc = ConvertToBsonDocument(entity);
        await _collection.InsertOneAsync(doc);
    }

    public async Task InsertManyAsync(IEnumerable<MongoProduct> entities)
    {
        var docs = entities.Select(ConvertToBsonDocument);
        await _collection.InsertManyAsync(docs);
    }

    public Task<bool> DeleteAsync(FilterDefinition<MongoProduct> filter)
    {
        throw new NotImplementedException("Use specific delete methods instead");
    }

    public Task<bool> DeleteManyAsync(FilterDefinition<MongoProduct> filter)
    {
        throw new NotImplementedException("Use specific delete methods instead");
    }

    public async Task<long> CountAsync(FilterDefinition<MongoProduct> filter)
    {
        return await _collection.CountDocumentsAsync(new BsonDocument());
    }

    public async Task<bool> ExistsAsync(Expression<Func<MongoProduct, bool>> filter)
    {
        var result = await GetFirstOrDefaultAsync(filter);
        return result != null;
    }

    // Helper methods
    private static MongoProduct ConvertToMongoProduct(BsonDocument doc)
    {
        return new MongoProduct
        {
            Id = doc["_id"].AsObjectId,
            ProductId = doc.GetValue("ProductID", 0).AsInt32,
            ProductName = doc.GetValue("ProductName", "").AsString,
            SupplierId = doc.Contains("SupplierID") && !doc["SupplierID"].IsBsonNull ? doc["SupplierID"].AsInt32 : null,
            CategoryId = doc.Contains("CategoryID") && !doc["CategoryID"].IsBsonNull ? doc["CategoryID"].AsInt32 : null,
            QuantityPerUnit = doc.GetValue("QuantityPerUnit", "").AsString,
            UnitPrice = GetSafeDecimal(doc, "UnitPrice"),
            UnitsInStock = doc.Contains("UnitsInStock") && !doc["UnitsInStock"].IsBsonNull ? doc["UnitsInStock"].AsInt32 : null,
            UnitsOnOrder = doc.Contains("UnitsOnOrder") && !doc["UnitsOnOrder"].IsBsonNull ? doc["UnitsOnOrder"].AsInt32 : null,
            ReorderLevel = doc.Contains("ReorderLevel") && !doc["ReorderLevel"].IsBsonNull ? doc["ReorderLevel"].AsInt32 : null,
            Discontinued = GetSafeBoolean(doc, "Discontinued"),
            GameKey = doc.GetValue("GameKey", "").AsString,
            ViewCount = doc.GetValue("ViewCount", 0).AsInt32
        };
    }

    private static decimal? GetSafeDecimal(BsonDocument doc, string fieldName)
    {
        if (!doc.Contains(fieldName) || doc[fieldName].IsBsonNull)
        {
            return null;
        }

        var value = doc[fieldName];

        // Handle different numeric types that MongoDB might use
        return value.BsonType switch
        {
            BsonType.Double => (decimal)value.AsDouble,
            BsonType.Decimal128 => value.AsDecimal,
            BsonType.Int32 => value.AsInt32,
            BsonType.Int64 => value.AsInt64,
            BsonType.EndOfDocument => throw new NotImplementedException(),
            BsonType.String => throw new NotImplementedException(),
            BsonType.Document => throw new NotImplementedException(),
            BsonType.Array => throw new NotImplementedException(),
            BsonType.Binary => throw new NotImplementedException(),
            BsonType.Undefined => throw new NotImplementedException(),
            BsonType.ObjectId => throw new NotImplementedException(),
            BsonType.Boolean => throw new NotImplementedException(),
            BsonType.DateTime => throw new NotImplementedException(),
            BsonType.Null => throw new NotImplementedException(),
            BsonType.RegularExpression => throw new NotImplementedException(),
            BsonType.JavaScript => throw new NotImplementedException(),
            BsonType.Symbol => throw new NotImplementedException(),
            BsonType.JavaScriptWithScope => throw new NotImplementedException(),
            BsonType.Timestamp => throw new NotImplementedException(),
            BsonType.MinKey => throw new NotImplementedException(),
            BsonType.MaxKey => throw new NotImplementedException(),
            _ => null
        };
    }

    private static bool GetSafeBoolean(BsonDocument doc, string fieldName)
    {
        if (!doc.Contains(fieldName) || doc[fieldName].IsBsonNull)
        {
            return false;
        }

        var value = doc[fieldName];

        // Handle different types that MongoDB might use for boolean
        return value.BsonType switch
        {
            BsonType.Boolean => value.AsBoolean,
            BsonType.Int32 => value.AsInt32 != 0,
            BsonType.Int64 => value.AsInt64 != 0,
            BsonType.Double => value.AsDouble != 0,
            BsonType.String => bool.TryParse(value.AsString, out var result) && result,
            BsonType.EndOfDocument => throw new NotImplementedException(),
            BsonType.Document => throw new NotImplementedException(),
            BsonType.Array => throw new NotImplementedException(),
            BsonType.Binary => throw new NotImplementedException(),
            BsonType.Undefined => throw new NotImplementedException(),
            BsonType.ObjectId => throw new NotImplementedException(),
            BsonType.DateTime => throw new NotImplementedException(),
            BsonType.Null => throw new NotImplementedException(),
            BsonType.RegularExpression => throw new NotImplementedException(),
            BsonType.JavaScript => throw new NotImplementedException(),
            BsonType.Symbol => throw new NotImplementedException(),
            BsonType.JavaScriptWithScope => throw new NotImplementedException(),
            BsonType.Timestamp => throw new NotImplementedException(),
            BsonType.Decimal128 => throw new NotImplementedException(),
            BsonType.MinKey => throw new NotImplementedException(),
            BsonType.MaxKey => throw new NotImplementedException(),
            _ => false
        };
    }

    private static BsonDocument ConvertToBsonDocument(MongoProduct product)
    {
        var doc = new BsonDocument
        {
            ["ProductID"] = product.ProductId,
            ["ProductName"] = product.ProductName,
            ["QuantityPerUnit"] = product.QuantityPerUnit,
            ["Discontinued"] = product.Discontinued,
            ["GameKey"] = product.GameKey,
            ["ViewCount"] = product.ViewCount
        };

        if (product.SupplierId.HasValue)
        {
            doc["SupplierID"] = product.SupplierId.Value;
        }
        if (product.CategoryId.HasValue)
        {
            doc["CategoryID"] = product.CategoryId.Value;
        }
        if (product.UnitPrice.HasValue)
        {
            doc["UnitPrice"] = product.UnitPrice.Value;
        }
        if (product.UnitsInStock.HasValue)
        {
            doc["UnitsInStock"] = product.UnitsInStock.Value;
        }
        if (product.UnitsOnOrder.HasValue)
        {
            doc["UnitsOnOrder"] = product.UnitsOnOrder.Value;
        }

        if (product.ReorderLevel.HasValue)
        {
            doc["ReorderLevel"] = product.ReorderLevel.Value;
        }

        return doc;
    }
}