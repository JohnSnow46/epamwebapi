using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Community;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;
public class BanRepository(GameCatalogDbContext context) : Repository<Ban>(context), IBanRepository
{
    private readonly GameCatalogDbContext _context = context;

    public async Task<Ban?> GetActiveBanByUserNameAsync(string userName)
    {
        var currentTime = DateTime.UtcNow;

        return await _context.Bans
            .Where(b => b.UserName.ToLower() == userName.ToLower() &&
                       (b.IsPermanent || b.BanEnd > currentTime))
            .OrderByDescending(b => b.BanStart)
            .FirstOrDefaultAsync();
    }


    public async Task<bool> IsUserBannedAsync(string userName)
    {
        var ban = await GetActiveBanByUserNameAsync(userName);
        return ban != null;
    }
}