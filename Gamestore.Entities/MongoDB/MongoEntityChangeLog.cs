#nullable disable
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Entities.MongoDB;

public class MongoEntityChangeLog
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("Timestamp")]
    public DateTime Timestamp { get; set; }

    [BsonElement("ActionName")]
    public string ActionName { get; set; }

    [BsonElement("EntityType")]
    public string EntityType { get; set; }

    [BsonElement("EntityId")]
    public string EntityId { get; set; }

    [BsonElement("OldVersion")]
    public BsonDocument OldVersion { get; set; }

    [BsonElement("NewVersion")]
    public BsonDocument NewVersion { get; set; }

    [BsonElement("UserId")]
    public string UserId { get; set; }

    [BsonElement("DatabaseSource")]
    public string DatabaseSource { get; set; }
}