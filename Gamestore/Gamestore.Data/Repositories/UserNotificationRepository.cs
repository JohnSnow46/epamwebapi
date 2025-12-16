using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

public class UserNotificationRepository(GameCatalogDbContext context) : IUserNotificationRepository
{
    private readonly GameCatalogDbContext _context = context;

    public async Task<IEnumerable<UserNotificationPreference>> GetUserNotificationPreferencesAsync(Guid userId)
    {
        return await _context.UserNotificationPreferences
            .Where(unp => unp.UserId == userId)
            .OrderBy(unp => unp.NotificationMethod)
            .ToListAsync();
    }

    public async Task<List<string>> GetUserEnabledNotificationMethodsAsync(Guid userId)
    {
        return await _context.UserNotificationPreferences
            .Where(unp => unp.UserId == userId && unp.IsEnabled)
            .Select(unp => unp.NotificationMethod)
            .OrderBy(m => m)
            .ToListAsync();
    }

    public async Task UpdateUserNotificationPreferencesAsync(Guid userId, List<string> notificationMethods)
    {
        // Remove all existing preferences for this user
        var existingPreferences = _context.UserNotificationPreferences
            .Where(unp => unp.UserId == userId);
        _context.UserNotificationPreferences.RemoveRange(existingPreferences);

        // Create new preferences for each method
        var newPreferences = notificationMethods
            .Select(method => new UserNotificationPreference
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                NotificationMethod = method.ToLowerInvariant(),
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            })
            .ToList();

        await _context.UserNotificationPreferences.AddRangeAsync(newPreferences);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsNotificationMethodEnabledAsync(Guid userId, string notificationMethod)
    {
        return await _context.UserNotificationPreferences
            .AnyAsync(unp => unp.UserId == userId
                && unp.NotificationMethod == notificationMethod.ToLowerInvariant()
                && unp.IsEnabled);
    }

    public async Task InitializeDefaultPreferencesAsync(Guid userId)
    {
        // Check if user already has preferences
        var existingCount = await _context.UserNotificationPreferences
            .CountAsync(unp => unp.UserId == userId);

        if (existingCount > 0)
        {
            return;
        }

        // Set all methods as enabled by default
        var defaultMethods = new[] { "sms", "push", "email" };
        await UpdateUserNotificationPreferencesAsync(userId, defaultMethods.ToList());
    }
}