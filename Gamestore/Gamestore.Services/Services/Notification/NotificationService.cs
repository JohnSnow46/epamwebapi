using Gamestore.Data.Interfaces;
using Gamestore.Services.Constants;
using Gamestore.Services.Dto.NotificationsDto;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.Notification;

public class NotificationService(
    IUnitOfWork unitOfWork,
    ILogger<NotificationService> logger) : INotificationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<NotificationService> _logger = logger;

    public async Task<List<string>> GetAvailableNotificationMethodsAsync()
    {
        _logger.LogInformation("Retrieving available notification methods");
        return await Task.FromResult(NotificationConstants.AvailableMethods);
    }

    public async Task<List<string>> GetUserNotificationMethodsAsync(Guid userId)
    {
        _logger.LogInformation("Retrieving notification methods for user: {UserId}", userId);

        var methods = await _unitOfWork.UserNotifications
            .GetUserEnabledNotificationMethodsAsync(userId);

        return methods;
    }

    public async Task<UserNotificationMethodsDto> UpdateUserNotificationMethodsAsync(
        Guid userId,
        List<string> notificationMethods)
    {
        _logger.LogInformation(
            "Updating notification methods for user {UserId}: {Methods}",
            userId,
            string.Join(", ", notificationMethods));

        var validMethods = notificationMethods
            .Where(m => NotificationConstants.AvailableMethods.Contains(m.ToLowerInvariant()))
            .Select(m => m.ToLowerInvariant())
            .Distinct()
            .ToList();

        await _unitOfWork.UserNotifications.UpdateUserNotificationPreferencesAsync(userId, validMethods);
        await _unitOfWork.CompleteAsync();

        return new UserNotificationMethodsDto
        {
            UserId = userId,
            SelectedMethods = validMethods,
            AvailableMethods = NotificationConstants.AvailableMethods,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}