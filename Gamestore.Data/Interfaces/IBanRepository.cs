using Gamestore.Entities.Community;

namespace Gamestore.Data.Interfaces;

/// <summary>
/// Repository interface for managing user ban records in the system.
/// Provides specialized methods for checking user ban status and retrieving active bans.
/// Extends the generic repository pattern with ban-specific operations.
/// </summary>
public interface IBanRepository : IRepository<Ban>
{
    /// <summary>
    /// Retrieves the currently active ban for a specific user by their username.
    /// An active ban is either permanent or has not yet expired.
    /// </summary>
    /// <param name="userName">The username of the user to check for active bans. Case-insensitive comparison is performed.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the active Ban entity 
    /// if found, or null if the user has no active bans.
    /// </returns>
    Task<Ban?> GetActiveBanByUserNameAsync(string userName);

    /// <summary>
    /// Determines whether a user is currently banned from the system.
    /// Checks for any active permanent or temporary bans that have not expired.
    /// </summary>
    /// <param name="userName">The username of the user to check ban status for. Case-insensitive comparison is performed.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is true if the user 
    /// has an active ban, false otherwise.
    /// </returns>
    Task<bool> IsUserBannedAsync(string userName);
}

