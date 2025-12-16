namespace Gamestore.Services.Interfaces;

/// <summary>
/// Interface for email service.
/// Epic 12 US4 - Email notification infrastructure.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send email to recipient.
    /// </summary>
    Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtmlBody = true);

    /// <summary>
    /// Send order notification email.
    /// </summary>
    Task<bool> SendOrderNotificationEmailAsync(string userEmail, string userName, string orderId, string orderStatus, decimal totalPrice);

    /// <summary>
    /// Send notification preferences confirmation email.
    /// </summary>
    Task<bool> SendNotificationPreferencesEmailAsync(string userEmail, string userName, List<string> selectedMethods);
}