using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

public class GameRepository(GameCatalogDbContext context) : Repository<Game>(context), IGameRepository
{
    private readonly GameCatalogDbContext _context = context;

    public async Task<Game?> GetKeyAsync(string key)
    {
        return await _context.Games
            .Include(g => g.GameGenres)
                .ThenInclude(gg => gg.Genre)
            .Include(g => g.GamePlatforms)
                .ThenInclude(gp => gp.Platform)
            .Include(g => g.Publisher)
            .FirstOrDefaultAsync(g => g.Key == key);
    }

    public async Task<Game> DeleteGameByKey(Game game)
    {
        _context.Games.Remove(game);
        await _context.SaveChangesAsync();

        return game;
    }

    public async Task<IEnumerable<Game>> GetByPlatformAsync(Guid platformId)
    {
        return await _context.GamePlatforms
                             .Where(gp => gp.PlatformId == platformId)
                             .Select(gp => gp.Game)
                             .ToListAsync();
    }

    public async Task<IEnumerable<Game>> GetByGenreAsync(Guid genreId)
    {
        return await _context.GameGenres
                             .Where(gg => gg.GenreId == genreId)
                             .Select(gg => gg.Game)
                             .ToListAsync();
    }

    public async Task<IEnumerable<Game>> GetByIdsAsync(List<Guid> gameIds)
    {
        return await _context.Games
            .Where(g => gameIds.Contains(g.Id))
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.Games.CountAsync();
    }

    public async Task IncrementViewCountAsync(string key)
    {
        var game = await _context.Games.FirstOrDefaultAsync(g => g.Key == key);

        if (game != null)
        {
            game.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }

    public override async Task<IEnumerable<Game>> GetAllAsync()
    {
        var games = await _context.Games
            .Include(g => g.GameGenres)
                .ThenInclude(gg => gg.Genre)
            .Include(g => g.GamePlatforms)
                .ThenInclude(gp => gp.Platform)
            .Include(g => g.Publisher)
            .ToListAsync();

        return games;
    }
}
