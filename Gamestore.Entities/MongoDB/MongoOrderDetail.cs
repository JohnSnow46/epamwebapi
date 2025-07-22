#nullable disable
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Entities.MongoDB;

public class MongoOrderDetail
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("OrderID")]
    public int OrderId { get; set; }

    [BsonElement("ProductID")]
    public int ProductId { get; set; }

    [BsonElement("UnitPrice")]
    public decimal UnitPrice { get; set; }

    [BsonElement("Quantity")]
    public int Quantity { get; set; }

    [BsonElement("Discount")]
    public float Discount { get; set; }
}