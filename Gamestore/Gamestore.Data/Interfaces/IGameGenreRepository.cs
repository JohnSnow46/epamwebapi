using Gamestore.Entities.Business;

namespace Gamestore.Data.Interfaces;

public interface IGameGenreRepository
{
    Task<List<Genre>> GetByIdsAsync(List<Guid> ids);

    Task RemoveRangeAsync(IEnumerable<GameGenre> gameGenres);

    Task AddRangeAsync(IEnumerable<GameGenre> gameGenres);

    Task<List<GameGenre>> GetByGameIdAsync(Guid gameId);

    Task<IEnumerable<GameGenre>> GetByGenreIdAsync(Guid genreId);
}
