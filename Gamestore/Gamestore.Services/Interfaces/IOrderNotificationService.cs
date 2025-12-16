namespace Gamestore.Services.Interfaces;

/// <summary>
/// Interface for order notification service.
/// Handles notifying users and managers about order status changes.
/// </summary>
public interface IOrderNotificationService
{
    /// <summary>
    /// Notify user and managers when order status changes.
    /// Epic 12 requirement: Managers and order owner get notifications on status change.
    /// </summary>
    Task NotifyOrderStatusChangedAsync(
        Guid orderId,
        string orderStatus,
        string userEmail,
        string userName,
        decimal totalPrice);

    /// <summary>
    /// Notify managers about new order.
    /// </summary>
    Task NotifyManagersAboutNewOrderAsync(
        Guid orderId,
        string userEmail,
        string userName,
        decimal totalPrice);
}