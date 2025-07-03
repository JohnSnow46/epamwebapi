using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

public class PublisherRepository(GameCatalogDbContext context) : Repository<Publisher>(context), IPublisherRepository
{
    private readonly GameCatalogDbContext _context = context;

    public async Task<Publisher?> GetByCompanyNameAsync(string companyName)
    {
        return await _context.Publishers
            .Include(p => p.Games)
            .FirstOrDefaultAsync(p => p.CompanyName == companyName);
    }

    public async Task<IEnumerable<Game>> GetGamesByPublisherIdAsync(Guid publisherId)
    {
        return await _context.Games
            .Where(g => g.PublisherId == publisherId)
            .ToListAsync();
    }
}