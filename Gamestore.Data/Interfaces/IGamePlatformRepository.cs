using Gamestore.Entities.Business;

namespace Gamestore.Data.Interfaces;

public interface IGamePlatformRepository
{
    Task<List<Platform>> GetByIdsAsync(List<Guid> ids);

    Task<List<GamePlatform>> GetByGameIdAsync(Guid gameId);

    Task RemoveRangeAsync(IEnumerable<GamePlatform> gamePlatforms);

    Task AddRangeAsync(IEnumerable<GamePlatform> gamePlatforms);

    Task<IEnumerable<GamePlatform>> GetByPlatformIdAsync(Guid platformId);
}
