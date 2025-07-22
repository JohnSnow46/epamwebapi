#nullable disable
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Entities.MongoDB;

/// <summary>
/// MongoDB model for Northwind Products collection
/// Maps to existing Northwind data with additional Game Store fields
/// </summary>
public class MongoProduct
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("ProductID")]
    public int ProductId { get; set; }

    [BsonElement("ProductName")]
    public string ProductName { get; set; }

    [BsonElement("SupplierID")]
    public int? SupplierId { get; set; }

    [BsonElement("CategoryID")]
    public int? CategoryId { get; set; }

    [BsonElement("QuantityPerUnit")]
    public string QuantityPerUnit { get; set; }

    [BsonElement("UnitPrice")]
    public decimal? UnitPrice { get; set; }

    [BsonElement("UnitsInStock")]
    public int? UnitsInStock { get; set; }

    [BsonElement("UnitsOnOrder")]
    public int? UnitsOnOrder { get; set; }

    [BsonElement("ReorderLevel")]
    public int? ReorderLevel { get; set; }

    [BsonElement("Discontinued")]
    public bool Discontinued { get; set; }

    [BsonElement("GameKey")]
    public string GameKey { get; set; }

    [BsonElement("ViewCount")]
    public int ViewCount { get; set; }
}
