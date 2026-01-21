using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Constants;

/// <summary>
/// Extension methods for NotificationMethod enum.
/// Replaces NotificationConstants functionality with type-safe approach.
/// </summary>
public static class NotificationMethodExtensions
{
    /// <summary>
    /// Gets the string display name for a notification method.
    /// Example: NotificationMethod.SMS.GetDisplayName() returns "sms".
    /// </summary>
    public static string GetDisplayName(this NotificationMethod method)
    {
        var field = method.GetType().GetField(method.ToString());
        var attribute = field?.GetCustomAttributes(typeof(DisplayAttribute), false)
            .FirstOrDefault() as DisplayAttribute;
        return attribute?.Name ?? method.ToString().ToLower();
    }

    /// <summary>
    /// Gets all available notification methods as strings.
    /// Returns: ["sms", "push", "email"]
    /// Replaces NotificationConstants.AvailableMethods.
    /// </summary>
    public static List<string> GetAvailableMethodsAsStrings()
    {
        return Enum.GetValues(typeof(NotificationMethod))
            .Cast<NotificationMethod>()
            .Select(m => m.GetDisplayName())
            .ToList();
    }

    /// <summary>
    /// Converts a string value to NotificationMethod enum.
    /// Example: "sms".ToNotificationMethod() returns NotificationMethod.SMS
    /// Returns null if string doesn't match any method.
    /// </summary>
    public static NotificationMethod? ToNotificationMethod(this string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : Enum.GetValues(typeof(NotificationMethod))
            .Cast<NotificationMethod>()
            .FirstOrDefault(m => m.GetDisplayName().Equals(value, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if a string value is a valid notification method.
    /// </summary>
    public static bool IsValidNotificationMethod(this string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && value.ToNotificationMethod().HasValue;
    }
}