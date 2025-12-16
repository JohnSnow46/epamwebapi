namespace Gamestore.Entities.Notifications;

/// <summary>
/// Represents user's notification method preference.
/// One record per notification method per user.
/// </summary>
public class UserNotificationPreference
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets get or sets notification method: sms, push, email.
    /// </summary>
    public string NotificationMethod { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether whether this method is enabled for the user.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}