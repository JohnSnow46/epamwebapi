using Gamestore.Entities.Business;

namespace Gamestore.Data.Interfaces;

/// <summary>
/// Repository interface for managing Publisher entities in the game catalog system.
/// Provides specialized operations for publisher management, company name lookups,
/// and retrieving games by publisher relationships.
/// Extends the generic repository pattern with publisher-specific business operations.
/// </summary>
public interface IPublisherRepository : IRepository<Publisher>
{
    /// <summary>
    /// Retrieves a publisher by their unique company name.
    /// Company names serve as business identifiers for publishers in the system.
    /// </summary>
    /// <param name="companyName">The unique company name of the publisher to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the Publisher entity
    /// if found, or null if no publisher with the specified company name exists.
    /// </returns>
    Task<Publisher?> GetByCompanyNameAsync(string companyName);

    /// <summary>
    /// Retrieves all games published by a specific publisher.
    /// This method queries the publisher-game relationship to return the complete catalog
    /// of games associated with the specified publisher.
    /// </summary>
    /// <param name="publisherId">The unique identifier of the publisher whose games to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Game entities
    /// published by the specified publisher. Returns an empty collection if the publisher has no games.
    /// </returns>
    Task<IEnumerable<Game>> GetGamesByPublisherIdAsync(Guid publisherId);
}