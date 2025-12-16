using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.Notification;

/// <summary>
/// Dispatcher for sending notifications via multiple channels (SMS, Push, Email).
/// Epic 12 US4 - Routes notifications to appropriate services.
/// </summary>
public class NotificationDispatcher(IEmailService emailService, ILogger<NotificationDispatcher> logger) : INotificationDispatcher
{
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<NotificationDispatcher> _logger = logger;

    /// <summary>
    /// Send notification via specified method.
    /// SMS and Push are faked (logged only), Email is real.
    /// </summary>
    public async Task<bool> SendNotificationAsync(
        string userEmail,
        string userName,
        string notificationMethod,
        string subject,
        string message)
    {
        try
        {
            _logger.LogInformation(
                "Sending {Method} notification to {Email}: {Subject}",
                notificationMethod,
                userEmail,
                subject);

            return notificationMethod.ToLowerInvariant() switch
            {
                "sms" => await SendSmsNotificationAsync(userEmail, subject, message),
                "push" => await SendPushNotificationAsync(userEmail, subject, message),
                "email" => await SendEmailNotificationAsync(userEmail, subject, message),
                _ => false,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending {Method} notification to {Email}", notificationMethod, userEmail);
            return false;
        }
    }

    /// <summary>
    /// Send order status notification via specified method.
    /// </summary>
    public async Task<bool> SendOrderStatusNotificationAsync(
        string userEmail,
        string userName,
        string notificationMethod,
        string orderId,
        string orderStatus,
        decimal totalPrice)
    {
        try
        {
            var subject = $"Order {orderId} - {orderStatus}";
            var message = $"Your order #{orderId} status: {orderStatus}. Total: ${totalPrice:F2}";

            return await SendNotificationAsync(userEmail, userName, notificationMethod, subject, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending order status notification to {Email}", userEmail);
            return false;
        }
    }

    /// <summary>
    /// Send SMS notification (FAKED - logged only).
    /// Epic 12: SMS should be faked for now.
    /// </summary>
    private async Task<bool> SendSmsNotificationAsync(string userEmail, string subject, string message)
    {
        try
        {
            _logger.LogInformation(
                "📱 [FAKED SMS] To: {Email}, Subject: {Subject}, Message: {Message}",
                userEmail,
                subject,
                message);

            // In real implementation, integrate with SMS provider (Twilio, AWS SNS, etc.)
            await Task.Delay(100); // Simulate async operation
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendSmsNotificationAsync");
            return false;
        }
    }

    /// <summary>
    /// Send Push notification (FAKED - logged only).
    /// Epic 12: Push notifications should be faked for now.
    /// </summary>
    private async Task<bool> SendPushNotificationAsync(string userEmail, string subject, string message)
    {
        try
        {
            _logger.LogInformation(
                "🔔 [FAKED PUSH] To: {Email}, Subject: {Subject}, Message: {Message}",
                userEmail,
                subject,
                message);

            // In real implementation, integrate with push notification service (Firebase, OneSignal, etc.)
            await Task.Delay(100); // Simulate async operation
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendPushNotificationAsync");
            return false;
        }
    }

    /// <summary>
    /// Send Email notification (REAL).
    /// Epic 12 US4: Email is implemented.
    /// </summary>
    private async Task<bool> SendEmailNotificationAsync(string userEmail, string subject, string message)
    {
        try
        {
            var htmlBody = $@"
                <h3>{subject}</h3>
                <p>{message}</p>
                <p>Best regards,<br/>Game Store Team</p>
            ";

            return await _emailService.SendEmailAsync(userEmail, subject, htmlBody, isHtmlBody: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendEmailNotificationAsync");
            return false;
        }
    }
}