using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

/// <summary>
/// Generic repository implementation providing basic CRUD operations for entity management.
/// Implements the Repository pattern to abstract data access logic and provide a consistent
/// interface for entity operations across the application with immediate change persistence.
/// </summary>
/// <typeparam name="T">The entity type that this repository manages. Must be a reference type (class).</typeparam>
public class Repository<T> : IRepository<T>
    where T : class
{
    private readonly GameCatalogDbContext _context;
    private readonly DbSet<T> _dbSet;

    /// <summary>
    /// Initializes a new instance of the Repository class with the specified database context.
    /// Sets up the DbSet for the entity type and prepares the repository for data operations.
    /// </summary>
    /// <param name="context">The Entity Framework database context to use for data operations.</param>
    public Repository(GameCatalogDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    /// <summary>
    /// Retrieves a single entity by its unique identifier.
    /// Throws a KeyNotFoundException if the entity is not found, ensuring consistent error handling.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the entity
    /// if found, or throws a KeyNotFoundException if no entity with the specified ID exists.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when no entity with the specified ID is found.</exception>
    public async Task<T> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id) ?? throw new KeyNotFoundException("Entity not found.");
    }

    /// <summary>
    /// Retrieves all entities of type T from the data store.
    /// This method is marked as virtual to allow derived repositories to override
    /// with specialized implementations that include related data or custom ordering.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection
    /// of all entities of type T. Returns an empty collection if no entities exist.
    /// </returns>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <summary>
    /// Adds a new entity to the data store and immediately persists the changes.
    /// This method performs both the add operation and the save operation in a single call
    /// for immediate data consistency.
    /// </summary>
    /// <param name="entity">The entity to add to the data store.</param>
    /// <returns>A task representing the asynchronous add and save operation.</returns>
    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing entity in the data store and immediately persists the changes.
    /// This method marks the entity as modified and saves the changes to ensure
    /// immediate data consistency and conflict detection.
    /// </summary>
    /// <param name="entity">The entity with updated values to save to the data store.</param>
    /// <returns>A task representing the asynchronous update and save operation.</returns>
    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes an entity from the data store by its unique identifier and immediately persists the changes.
    /// This method first retrieves the entity, then removes it if found, ensuring referential integrity.
    /// No operation is performed if the entity doesn't exist.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <returns>A task representing the asynchronous delete and save operation.</returns>
    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}