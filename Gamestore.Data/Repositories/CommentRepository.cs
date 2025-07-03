using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Community;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;
public class CommentRepository(GameCatalogDbContext context) : Repository<Comment>(context), ICommentRepository
{
    private readonly GameCatalogDbContext _context = context;

    public async Task<IEnumerable<Comment>> GetCommentsByGameKeyAsync(string gameKey)
    {
        var game = await _context.Games
            .FirstOrDefaultAsync(g => g.Key == gameKey);

        return game == null ? Enumerable.Empty<Comment>() : await GetRootCommentsByGameIdAsync(game.Id);
    }

    public async Task<Comment?> GetCommentByIdAsync(Guid id)
    {
        return await _context.Comments
            .Include(c => c.ChildComments)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Comment>> GetRootCommentsByGameIdAsync(Guid gameId)
    {
        return await _context.Comments
            .Where(c => c.GameId == gameId && c.ParentCommentId == null)
            .Include(c => c.ChildComments)
            .ToListAsync();
    }
}
