using System.Text;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Notifications;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.Notification;

public class OrderNotificationService(
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    ILogger<OrderNotificationService> logger) : IOrderNotificationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<OrderNotificationService> _logger = logger;

    public async Task NotifyOrderStatusChangedAsync(
        Guid orderId,
        string orderStatus,
        string userEmail,
        string userName,
        decimal totalPrice)
    {
        _logger.LogInformation(
            "Starting notify order status changed for order: {OrderId}, Status: {Status}", orderId, orderStatus);

        try
        {
            var emailContent = BuildOrderStatusEmailHtml(orderId, orderStatus, userName, totalPrice);
            var recipients = new List<string> { userEmail };

            var managers = await GetManagerEmails();
            recipients.AddRange(managers);

            foreach (var email in recipients)
            {
                try
                {
                    await _emailService.SendEmailAsync(
                        email,
                        $"Order {orderId} Status: {orderStatus}",
                        emailContent,
                        isHtml: true);

                    var notification = new OrderNotification
                    {
                        OrderId = orderId,
                        RecipientEmail = email,
                        NotificationType = "Email",
                        SentAt = DateTime.UtcNow,
                    };

                    await _unitOfWork.OrderNotifications.AddAsync(notification);
                    _logger.LogInformation("✅ Status notification sent to: {Email}", email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to send status notification to: {Email}", email);
                }
            }

            await _unitOfWork.CompleteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in NotifyOrderStatusChangedAsync for order: {OrderId}", orderId);
        }
    }

    public async Task NotifyManagersAboutNewOrderAsync(
        Guid orderId,
        string userEmail,
        string userName,
        decimal totalPrice)
    {
        _logger.LogInformation(
            "Starting notify managers about new order: {OrderId}",
            orderId);

        try
        {
            var managers = await GetManagerEmails();

            if (managers.Count == 0)
            {
                _logger.LogWarning("No managers found to notify about new order: {OrderId}", orderId);
                return;
            }

            var emailContent = BuildNewOrderManagerEmailHtml(orderId, userEmail, userName, totalPrice);

            foreach (var managerEmail in managers)
            {
                try
                {
                    await _emailService.SendEmailAsync(
                        managerEmail,
                        $"New Order {orderId} from {userName}",
                        emailContent,
                        isHtml: true);

                    var notification = new OrderNotification
                    {
                        OrderId = orderId,
                        RecipientEmail = managerEmail,
                        NotificationType = "Email",
                        SentAt = DateTime.UtcNow,
                    };

                    await _unitOfWork.OrderNotifications.AddAsync(notification);
                    _logger.LogInformation("✅ New order notification sent to manager: {Email}", managerEmail);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to send new order notification to manager: {Email}", managerEmail);
                }
            }

            await _unitOfWork.CompleteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in NotifyManagersAboutNewOrderAsync for order: {OrderId}", orderId);
        }
    }

    private static string BuildOrderStatusEmailHtml(
        Guid orderId,
        string orderStatus,
        string customerName,
        decimal totalPrice)
    {
        var html = new StringBuilder();
        html.Append("<html><body style='font-family: Arial, sans-serif;'>");
        html.Append($"<h2>Your Order Status Updated</h2>");
        html.Append($"<p>Dear {customerName},</p>");
        html.Append($"<p>Your order <strong>#{orderId}</strong> status: <strong>{orderStatus}</strong></p>");
        html.Append($"<p>Total Price: <strong>${totalPrice:F2}</strong></p>");
        html.Append($"<p>Updated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>");
        html.Append("<p>Thank you!</p>");
        html.Append("</body></html>");
        return html.ToString();
    }

    private static string BuildNewOrderManagerEmailHtml(
        Guid orderId,
        string userEmail,
        string userName,
        decimal totalPrice)
    {
        var html = new StringBuilder();
        html.Append("<html><body style='font-family: Arial, sans-serif;'>");
        html.Append($"<h2>New Order Received</h2>");
        html.Append($"<p>Order ID: {orderId}</p>");
        html.Append($"<p>Customer: {userName} ({userEmail})</p>");
        html.Append($"<p>Total Price: ${totalPrice:F2}</p>");
        html.Append($"<p>Received: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>");
        html.Append("</body></html>");
        return html.ToString();
    }

    private async Task<List<string>> GetManagerEmails()
    {
        try
        {
            var managers = await _unitOfWork.Users.GetAllAsync();
            return managers
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name is "Manager" or "Admin"))
                .Select(u => u.Email)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting manager emails");
            return new List<string>();
        }
    }
}