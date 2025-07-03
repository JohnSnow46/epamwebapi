using Gamestore.Entities.Business;

namespace Gamestore.Data.Interfaces;
public interface IGameRepository : IRepository<Game>
{
    Task<Game?> GetKeyAsync(string key);

    Task<Game> DeleteGameByKey(Game game);

    Task<IEnumerable<Game>> GetByPlatformAsync(Guid platformId);

    Task<IEnumerable<Game>> GetByGenreAsync(Guid genreId);

    Task<IEnumerable<Game>> GetByIdsAsync(List<Guid> gameIds);

    Task<int> CountAsync();
    new Task<IEnumerable<Game>> GetAllAsync();

    // ViewCount
    Task IncrementViewCountAsync(string key);
}
