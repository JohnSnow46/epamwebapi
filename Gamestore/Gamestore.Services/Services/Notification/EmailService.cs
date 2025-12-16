using System.Net;
using System.Net.Mail;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.Notification;

/// <summary>
/// Service for sending emails via SMTP.
/// Epic 12 US4 - Email notification infrastructure.
/// </summary>
public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<EmailService> _logger = logger;

    /// <summary>
    /// Send email to recipient.
    /// </summary>
    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtmlBody = true)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogWarning("SendEmailAsync called with empty recipient email");
                return false;
            }

            _logger.LogInformation("Sending email to: {ToEmail}, Subject: {Subject}", toEmail, subject);

            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var host = smtpSettings["Host"];
            var port = int.Parse(smtpSettings["Port"] ?? "587");
            var senderEmail = smtpSettings["SenderEmail"];
            var senderPassword = smtpSettings["SenderPassword"];
            var senderName = smtpSettings["SenderName"] ?? "Game Store";

            // For development: if SMTP not configured, log and return success
            if (string.IsNullOrWhiteSpace(host))
            {
                _logger.LogWarning("SMTP not configured, email would be sent to: {ToEmail}", toEmail);
                return true; // Return true so it doesn't block flow
            }

            using var client = new SmtpClient(host, port);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(senderEmail, senderPassword);
            client.Timeout = 10000;

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail!, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtmlBody,
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);

            _logger.LogInformation("Email successfully sent to: {ToEmail}", toEmail);
            return true;
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error sending email to: {ToEmail}", toEmail);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to: {ToEmail}", toEmail);
            return false;
        }
    }

    /// <summary>
    /// Send order notification email.
    /// </summary>
    public async Task<bool> SendOrderNotificationEmailAsync(string userEmail, string userName, string orderId, string orderStatus, decimal totalPrice)
    {
        var subject = $"Order {orderId} - Status Update: {orderStatus}";

        var body = $@"
            <h2>Order Status Update</h2>
            <p>Hello {userName},</p>
            <p>Your order <strong>#{orderId}</strong> status has been updated to <strong>{orderStatus}</strong>.</p>
            <p><strong>Total:</strong> ${totalPrice:F2}</p>
            <p>Thank you for your purchase!</p>
            <p>Game Store Team</p>
        ";

        return await SendEmailAsync(userEmail, subject, body, isHtmlBody: true);
    }

    /// <summary>
    /// Send notification preferences confirmation email.
    /// </summary>
    public async Task<bool> SendNotificationPreferencesEmailAsync(string userEmail, string userName, List<string> selectedMethods)
    {
        var subject = "Notification Preferences Updated";
        var methodsList = string.Join(", ", selectedMethods.Select(m => m.ToUpper()));

        var body = $@"
            <h2>Notification Preferences Updated</h2>
            <p>Hello {userName},</p>
            <p>Your notification preferences have been successfully updated.</p>
            <p><strong>Active notification methods:</strong> {(string.IsNullOrEmpty(methodsList) ? "None (you will not receive notifications)" : methodsList)}</p>
            <p>You can change these settings anytime in your account preferences.</p>
            <p>Game Store Team</p>
        ";

        return await SendEmailAsync(userEmail, subject, body, isHtmlBody: true);
    }
}