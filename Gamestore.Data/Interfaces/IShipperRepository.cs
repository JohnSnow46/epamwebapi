using MongoDB.Bson;

namespace Gamestore.Data.Interfaces;

public interface IShipperRepository
{
    Task<IEnumerable<BsonDocument>> GetAllShippersAsync();
    Task<BsonDocument?> GetShipperByIdAsync(int shipperId);
}