using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Community;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

/// <summary>
/// Repository implementation for managing Ban entities in the community moderation system.
/// Provides concrete implementations for ban-related operations including active ban checking,
/// user ban status validation, and ban lifecycle management.
/// Inherits from the generic Repository pattern and implements IBanRepository interface.
/// </summary>
public class BanRepository(GameCatalogDbContext context) : Repository<Ban>(context), IBanRepository
{
    private readonly GameCatalogDbContext _context = context;

    /// <summary>
    /// Retrieves the currently active ban for a specific user by their username.
    /// Performs case-insensitive username comparison and filters for bans that are either
    /// permanent or have not yet expired. Returns the most recent ban if multiple exist.
    /// </summary>
    /// <param name="userName">The username of the user to check for active bans.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the active Ban entity
    /// if found, or null if the user has no active bans.
    /// </returns>
    public async Task<Ban?> GetActiveBanByUserNameAsync(string userName)
    {
        var currentTime = DateTime.UtcNow;

        return await _context.Bans
            .Where(b => b.UserName.ToLower() == userName.ToLower() &&
                       (b.IsPermanent || b.BanEnd > currentTime))
            .OrderByDescending(b => b.BanStart)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Determines whether a user is currently banned from the system.
    /// This method provides a simple boolean check by leveraging the GetActiveBanByUserNameAsync method
    /// to determine ban status without exposing ban details.
    /// </summary>
    /// <param name="userName">The username of the user to check ban status for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is true if the user
    /// has an active ban, false otherwise.
    /// </returns>
    public async Task<bool> IsUserBannedAsync(string userName)
    {
        var ban = await GetActiveBanByUserNameAsync(userName);
        return ban != null;
    }
}