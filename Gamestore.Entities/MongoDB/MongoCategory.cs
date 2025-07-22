#nullable disable
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Entities.MongoDB;

public class MongoCategory
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("CategoryID")]
    public int CategoryId { get; set; }

    [BsonElement("CategoryName")]
    public string CategoryName { get; set; }

    [BsonElement("Description")]
    public string Description { get; set; }

    [BsonElement("Picture")]
    public byte[] Picture { get; set; }
}