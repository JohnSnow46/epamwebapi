namespace Gamestore.Services.Interfaces;

/// <summary>
/// Service interface for cache management operations.
/// Provides abstraction for cache implementation (Memory, Redis, etc).
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Attempts to retrieve a value from cache.
    /// </summary>
    /// <typeparam name="T">Type of cached value.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="value">Output: cached value if found.</param>
    /// <returns>True if value found in cache, false otherwise.</returns>
    bool TryGetValue<T>(string key, out T value);

    /// <summary>
    /// Sets a value in cache with specified duration.
    /// </summary>
    /// <typeparam name="T">Type of value to cache.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="value">Value to cache.</param>
    /// <param name="duration">Cache duration. If null, uses default 30 minutes.</param>
    void Set<T>(string key, T value, TimeSpan? duration = null);

    /// <summary>
    /// Removes a specific value from cache.
    /// </summary>
    /// <param name="key">Cache key to remove.</param>
    void Remove(string key);

    /// <summary>
    /// Removes multiple cache keys at once.
    /// Useful after update operations that affect multiple cached entities.
    /// </summary>
    /// <param name="keys">Variable number of cache keys to remove.</param>
    void RemoveMultiple(params string[] keys);

    /// <summary>
    /// Removes all cache keys matching a pattern (prefix-based).
    /// Example: RemoveByPattern("cache_game_") removes all game-related cache.
    /// </summary>
    /// <param name="pattern">Key pattern prefix.</param>
    void RemoveByPattern(string pattern);

    /// <summary>
    /// Clears entire cache. Use with caution!
    /// This is a heavy operation - use only when necessary.
    /// </summary>
    void Clear();
}