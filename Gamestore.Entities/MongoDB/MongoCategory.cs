using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Entities.MongoDB;

/// <summary>
/// MongoDB model for Northwind Categories collection
/// Maps to Genre in Game Store system
/// </summary>
public class MongoCategory
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("CategoryID")]
    public int CategoryId { get; set; }

    [BsonElement("CategoryName")]
    public string CategoryName { get; set; } = string.Empty;

    [BsonElement("Description")]
    public string? Description { get; set; }

    [BsonElement("Picture")]
    public byte[]? Picture { get; set; }
}