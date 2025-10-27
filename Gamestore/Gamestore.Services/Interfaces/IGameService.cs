using Gamestore.Entities.Business;
using Gamestore.Services.Dto.GamesDto;

namespace Gamestore.Services.Interfaces;

public interface IGameService
{
    Task<GameMetadataCreateRequestDto> AddGameAsync(GameMetadataCreateRequestDto gameRequest);

    Task<GameUpdateRequestDto> UpdateGameAsync(string key, GameMetadataUpdateRequestDto gameRequest);

    Task<GameUpdateRequestDto> GetGameByKey(string key);

    Task<GameCreateRequestDto> GetGameById(Guid id);

    Task<Game> DeleteGameAsync(string key);

    Task<IEnumerable<GameCreateRequestDto>> GetAllGames();

    Task<string> CreateGameFileAsync(string gameKey);

    Task<IEnumerable<GameCreateRequestDto>> GetGamesByPlatformAsync(Guid platformId);

    Task<IEnumerable<GameCreateRequestDto>> GetGamesByGenreAsync(Guid genreId);

    Task<int> GetTotalGamesCountAsync();
}
