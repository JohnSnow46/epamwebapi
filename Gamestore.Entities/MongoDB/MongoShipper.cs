#nullable disable
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Entities.MongoDB;

public class MongoShipper
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("ShipperID")]
    public int ShipperId { get; set; }

    [BsonElement("CompanyName")]
    public string CompanyName { get; set; }

    [BsonElement("Phone")]
    public string Phone { get; set; }
}