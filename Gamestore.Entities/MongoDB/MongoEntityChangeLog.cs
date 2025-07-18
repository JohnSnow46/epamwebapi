using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Entities.MongoDB;

/// <summary>
/// MongoDB model for logging all entity changes (E08 NFR4 requirement)
/// Logs changes from both SQL and MongoDB databases
/// </summary>
public class MongoEntityChangeLog
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("Timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("ActionName")]
    public string ActionName { get; set; } = string.Empty;

    [BsonElement("EntityType")]
    public string EntityType { get; set; } = string.Empty;

    [BsonElement("EntityId")]
    public string? EntityId { get; set; }

    [BsonElement("OldVersion")]
    public BsonDocument? OldVersion { get; set; }

    [BsonElement("NewVersion")]
    public BsonDocument? NewVersion { get; set; }

    [BsonElement("UserId")]
    public string? UserId { get; set; }

    [BsonElement("DatabaseSource")]
    public string DatabaseSource { get; set; } = string.Empty; // "SQL" or "MongoDB"
}