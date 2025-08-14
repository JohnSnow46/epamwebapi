using MongoDB.Bson;

namespace Gamestore.Data.MongoDB;

public interface IShipperRepository
{
    Task<IEnumerable<BsonDocument>> GetAllShippersAsync();
    Task<BsonDocument?> GetShipperByIdAsync(int shipperId);
}