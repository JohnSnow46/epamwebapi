namespace Gamestore.Services.Interfaces;

/// <summary>
/// Interface for notification dispatcher.
/// Routes notifications to SMS (faked), Push (faked), and Email (real).
/// </summary>
public interface INotificationDispatcher
{
    /// <summary>
    /// Send notification via specified method.
    /// </summary>
    Task<bool> SendNotificationAsync(
        string userEmail,
        string userName,
        string notificationMethod,
        string subject,
        string message);

    /// <summary>
    /// Send order status notification via specified method.
    /// </summary>
    Task<bool> SendOrderStatusNotificationAsync(
        string userEmail,
        string userName,
        string notificationMethod,
        string orderId,
        string orderStatus,
        decimal totalPrice);
}