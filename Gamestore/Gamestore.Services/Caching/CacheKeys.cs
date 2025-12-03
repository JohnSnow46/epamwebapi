namespace Gamestore.Services.Caching;

/// <summary>
/// Central cache key management for the entire application.
/// Ensures consistency and prevents key conflicts.
/// </summary>
#pragma warning disable SA1201 // Elements should appear in the correct order
public static class CacheKeys
{
    // ===== GAME CACHE KEYS =====
    public const string AllGames = "cache_all_games";
    public const string AllGamesCount = "cache_all_games_count";

    public static string GameByKey(string key) => $"cache_game_key_{key}";

    public static string GameById(Guid id) => $"cache_game_id_{id}";

    public static string GamesByGenre(Guid genreId) => $"cache_games_genre_{genreId}";

    public static string GamesByPlatform(Guid platformId) => $"cache_games_platform_{platformId}";

    public static string GamesByPublisher(Guid publisherId) => $"cache_games_publisher_{publisherId}";

    // ===== GENRE CACHE KEYS =====
    public const string AllGenres = "cache_all_genres";

    public static string GenreById(Guid id) => $"cache_genre_id_{id}";

    public static string GenresByGame(Guid gameId) => $"cache_genres_game_{gameId}";

    public static string GenresByParent(Guid? parentId) => $"cache_genres_parent_{parentId}";

    // ===== PLATFORM CACHE KEYS =====
    public const string AllPlatforms = "cache_all_platforms";

    public static string PlatformById(Guid id) => $"cache_platform_id_{id}";

    public static string PlatformsByGame(Guid gameId) => $"cache_platforms_game_{gameId}";

    // ===== PUBLISHER CACHE KEYS =====
    public const string AllPublishers = "cache_all_publishers";

    public static string PublisherById(Guid id) => $"cache_publisher_id_{id}";

    public static string PublisherByName(string name) => $"cache_publisher_name_{name}";

    // ===== COMMENT CACHE KEYS =====
    public static string CommentsByGame(Guid gameId) => $"cache_comments_game_{gameId}";

    public static string CommentById(Guid id) => $"cache_comment_id_{id}";

    public static string CommentsByGameKey(string gameKey) => $"cache_comments_game_key_{gameKey}";

    // ===== CACHE DURATIONS =====

    /// <summary>
    /// No caching - images should always be fresh from storage.
    /// Image URLs change when image is updated, but we don't cache the image itself.
    /// </summary>
    public const int ImageCacheDurationSeconds = 0;

    /// <summary>
    /// Cache duration for game lists (30 minutes).
    /// Lists change less frequently, longer cache is acceptable.
    /// </summary>
    public static readonly TimeSpan GamesCacheDuration = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Cache duration for single game details (1 hour).
    /// Individual game details are queried frequently, longer cache is safe.
    /// </summary>
    public static readonly TimeSpan SingleGameCacheDuration = TimeSpan.FromHours(1);

    /// <summary>
    /// Cache duration for genres (2 hours).
    /// Genres change very rarely.
    /// </summary>
    public static readonly TimeSpan GenresCacheDuration = TimeSpan.FromHours(2);

    /// <summary>
    /// Cache duration for platforms (2 hours).
    /// Platforms change very rarely.
    /// </summary>
    public static readonly TimeSpan PlatformsCacheDuration = TimeSpan.FromHours(2);

    /// <summary>
    /// Cache duration for publishers (2 hours).
    /// Publishers change rarely.
    /// </summary>
    public static readonly TimeSpan PublishersCacheDuration = TimeSpan.FromHours(2);

    /// <summary>
    /// Cache duration for comments (15 minutes).
    /// Comments can be added/deleted by users frequently.
    /// </summary>
    public static readonly TimeSpan CommentsCacheDuration = TimeSpan.FromMinutes(15);
}
