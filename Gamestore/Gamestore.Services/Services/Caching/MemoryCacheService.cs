using System.Collections.Concurrent;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.Caching;

/// <summary>
/// In-memory cache service implementation.
/// Uses IMemoryCache from Microsoft.Extensions.Caching.Memory.
/// Suitable for single-server deployments. For distributed systems, use Redis implementation.
/// </summary>
public class MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger) : ICacheService
{
    private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly ILogger<MemoryCacheService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ConcurrentDictionary<string, byte> _keyRegistry = new();

    /// <summary>
    /// Attempts to retrieve a value from cache.
    /// Logs cache hits/misses for debugging.
    /// </summary>
    public bool TryGetValue<T>(string key, out T value)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("TryGetValue called with null or empty key");
                value = default!;
                return false;
            }

            var found = _cache.TryGetValue(key, out T cachedValue);

            if (found)
            {
                _logger.LogDebug("Cache HIT for key: {CacheKey}", key);
            }
            else
            {
                _logger.LogDebug("Cache MISS for key: {CacheKey}", key);
            }

            value = cachedValue!;
            return found;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving value from cache for key: {CacheKey}", key);
            value = default!;
            return false;
        }
    }

    /// <summary>
    /// Sets a value in cache with optional duration.
    /// If duration is not specified, uses default 30 minutes.
    /// </summary>
    public void Set<T>(string key, T value, TimeSpan? duration = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Set called with null or empty key");
                return;
            }

            var options = new MemoryCacheEntryOptions();

            if (duration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = duration.Value;
                _logger.LogDebug(
                    "Cache SET for key: {CacheKey} with duration: {DurationMs}ms",
                    key,
                    duration.Value.TotalMilliseconds);
            }
            else
            {
                // Default 30 minutes if not specified
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                _logger.LogDebug(
                    "Cache SET for key: {CacheKey} with DEFAULT duration: 30 minutes",
                    key);
            }

            _cache.Set(key, value, options);

            // Registry key for pattern removal
            _keyRegistry.TryAdd(key, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache for key: {CacheKey}", key);
        }
    }

    /// <summary>
    /// Removes a specific key from cache.
    /// </summary>
    public void Remove(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Remove called with null or empty key");
                return;
            }

            _cache.Remove(key);
            _keyRegistry.TryRemove(key, out _);
            _logger.LogDebug("Cache REMOVED for key: {CacheKey}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache for key: {CacheKey}", key);
        }
    }

    /// <summary>
    /// Removes multiple keys from cache at once.
    /// More efficient than calling Remove() multiple times.
    /// </summary>
    public void RemoveMultiple(params string[] keys)
    {
        if (keys == null || keys.Length == 0)
        {
            _logger.LogWarning("RemoveMultiple called with null or empty keys");
            return;
        }

        try
        {
            var removedCount = 0;
            foreach (var key in keys)
            {
                if (!string.IsNullOrWhiteSpace(key))
                {
                    _cache.Remove(key);
                    _keyRegistry.TryRemove(key, out _);
                    removedCount++;
                }
            }

            _logger.LogDebug("Cache REMOVED for {Count} keys", removedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing multiple cache keys");
        }
    }

    /// <summary>
    /// Removes all keys matching a pattern (prefix-based).
    /// Example: RemoveByPattern("cache_game_") removes cache_game_key_*, cache_game_id_*, etc.
    /// Note: This operation iterates through registered keys, so efficiency depends on key count.
    /// </summary>
    public void RemoveByPattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            _logger.LogWarning("RemoveByPattern called with null or empty pattern");
            return;
        }

        try
        {
            var keysToRemove = _keyRegistry.Keys
                .Where(k => k.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                _keyRegistry.TryRemove(key, out _);
            }

            _logger.LogDebug("Cache REMOVED {Count} keys matching pattern: {Pattern}", keysToRemove.Count, pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache by pattern: {Pattern}", pattern);
        }
    }

    /// <summary>
    /// Clears entire cache. Use with caution!
    /// This is a heavy operation and should only be used when necessary.
    /// </summary>
    public void Clear()
    {
        try
        {
            _logger.LogWarning("CLEARING ENTIRE CACHE - this is a heavy operation!");

            var keys = _keyRegistry.Keys.ToList();
            foreach (var key in keys)
            {
                _cache.Remove(key);
            }

            _keyRegistry.Clear();
            _logger.LogInformation("Cache CLEARED successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing entire cache");
        }
    }
}