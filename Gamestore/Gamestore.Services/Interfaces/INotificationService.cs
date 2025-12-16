using Gamestore.Services.Dto.NotificationsDto;

namespace Gamestore.Services.Interfaces;

public interface INotificationService
{
    /// <summary>
    /// E12 US1 - Get all available notification methods.
    /// Returns: ["sms", "push", "email"].
    /// </summary>
    Task<List<string>> GetAvailableNotificationMethodsAsync();

    /// <summary>
    /// E12 US2 - Get user's selected notification methods.
    /// Returns: ["push", "email"].
    /// </summary>
    Task<List<string>> GetUserNotificationMethodsAsync(Guid userId);

    /// <summary>
    /// E12 US3 - Update user's notification method preferences.
    /// </summary>
    Task<UserNotificationMethodsDto> UpdateUserNotificationMethodsAsync(
        Guid userId,
        List<string> notificationMethods);
}