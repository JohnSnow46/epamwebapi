using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

/// <summary>
/// Repository implementation for managing Publisher entities in the game catalog system.
/// Provides concrete implementations for publisher management, company name lookups,
/// and retrieving games by publisher relationships with comprehensive business operations.
/// Inherits from the generic Repository pattern and implements IPublisherRepository interface.
/// </summary>
public class PublisherRepository(GameCatalogDbContext context) : Repository<Publisher>(context), IPublisherRepository
{
    private readonly GameCatalogDbContext _context = context;

    /// <summary>
    /// Retrieves a publisher by their unique company name with associated games eagerly loaded.
    /// Company names serve as business identifiers for publishers in the system.
    /// This method includes the publisher's game catalog for comprehensive publisher information.
    /// </summary>
    /// <param name="companyName">The unique company name of the publisher to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the Publisher entity
    /// with games loaded if found, or null if no publisher with the specified company name exists.
    /// </returns>
    public async Task<Publisher?> GetByCompanyNameAsync(string companyName)
    {
        return await _context.Publishers
            .Include(p => p.Games)
            .FirstOrDefaultAsync(p => p.CompanyName == companyName);
    }

    /// <summary>
    /// Retrieves all games published by a specific publisher.
    /// This method queries the publisher-game relationship to return the complete catalog
    /// of games associated with the specified publisher for browsing and management operations.
    /// </summary>
    /// <param name="publisherId">The unique identifier of the publisher whose games to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Game entities
    /// published by the specified publisher. Returns an empty collection if the publisher has no games.
    /// </returns>
    public async Task<IEnumerable<Game>> GetGamesByPublisherIdAsync(Guid publisherId)
    {
        return await _context.Games
            .Where(g => g.PublisherId == publisherId)
            .ToListAsync();
    }
}