using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Entities.MongoDB;

/// <summary>
/// MongoDB model for Northwind Shippers collection
/// Dynamic content structure as per Epic 8 US1 requirements
/// </summary>
public class MongoShipper
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("ShipperID")]
    public int ShipperId { get; set; }

    [BsonElement("CompanyName")]
    public string CompanyName { get; set; } = string.Empty;

    [BsonElement("Phone")]
    public string Phone { get; set; } = string.Empty;
}