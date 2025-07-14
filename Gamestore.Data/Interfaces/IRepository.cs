namespace Gamestore.Data.Interfaces;

/// <summary>
/// Generic repository interface providing basic CRUD operations for entity management.
/// Implements the Repository pattern to abstract data access logic and provide a consistent
/// interface for entity operations across the application.
/// </summary>
/// <typeparam name="T">The entity type that this repository manages. Must be a reference type (class).</typeparam>
public interface IRepository<T>
    where T : class
{
    /// <summary>
    /// Retrieves a single entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the entity
    /// if found, or throws an exception if no entity with the specified ID exists.
    /// </returns>
    Task<T> GetByIdAsync(Guid id);

    /// <summary>
    /// Retrieves all entities of type T from the data store.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection
    /// of all entities of type T. Returns an empty collection if no entities exist.
    /// </returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Adds a new entity to the data store.
    /// </summary>
    /// <param name="entity">The entity to add to the data store.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    Task AddAsync(T entity);

    /// <summary>
    /// Updates an existing entity in the data store.
    /// </summary>
    /// <param name="entity">The entity with updated values to save to the data store.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Deletes an entity from the data store by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    Task DeleteAsync(Guid id);
}