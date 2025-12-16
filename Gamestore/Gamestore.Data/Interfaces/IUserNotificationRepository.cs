using Gamestore.Entities.Notifications;

namespace Gamestore.Data.Interfaces;

public interface IUserNotificationRepository
{
    /// <summary>
    /// Get all notification preferences for a specific user.
    /// </summary>
    Task<IEnumerable<UserNotificationPreference>> GetUserNotificationPreferencesAsync(Guid userId);

    /// <summary>
    /// Get enabled notification methods for a specific user.
    /// </summary>
    Task<List<string>> GetUserEnabledNotificationMethodsAsync(Guid userId);

    /// <summary>
    /// Save or update user notification preferences.
    /// Deletes existing and creates new for each method.
    /// </summary>
    Task UpdateUserNotificationPreferencesAsync(Guid userId, List<string> notificationMethods);

    /// <summary>
    /// Check if a specific notification method is enabled for a user.
    /// </summary>
    Task<bool> IsNotificationMethodEnabledAsync(Guid userId, string notificationMethod);

    /// <summary>
    /// Initialize default notification preferences for a new user.
    /// </summary>
    Task InitializeDefaultPreferencesAsync(Guid userId);
}