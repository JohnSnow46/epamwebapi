using System.Net;
using System.Net.Mail;
using System.Text;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.Notification;

public class EmailService(
    IConfiguration configuration,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<EmailService> _logger = logger;

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtmlBody = true, bool isHtml = false)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("EmailSettings");
            var smtpHost = smtpSettings["SmtpHost"];
            var smtpPort = int.Parse(smtpSettings["SmtpPort"] ?? "587");
            var smtpUser = smtpSettings["SmtpUser"];
            var smtpPassword = smtpSettings["SmtpPassword"];
            var fromEmail = smtpSettings["FromEmail"];

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) ||
            string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(fromEmail))
            {
                _logger.LogError("❌ Email settings are not configured properly");
                return false;
            }

            _logger.LogInformation("Sending email to: {Email}, Subject: {Subject}", toEmail, subject);

            using var client = new SmtpClient(smtpHost, smtpPort);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(smtpUser, smtpPassword);

            using var message = new MailMessage(fromEmail, toEmail);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isHtmlBody;

            await client.SendMailAsync(message);
            _logger.LogInformation("✅ Email sent successfully to: {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send email to: {Email}", toEmail);
            return false;
        }
    }

    public async Task<bool> SendOrderNotificationEmailAsync(string userEmail, string userName, string orderId, string orderStatus, decimal totalPrice)
    {
        try
        {
            var subject = $"Order {orderId} Status Update";
            var body = BuildOrderNotificationHtml(userName, orderId, orderStatus, totalPrice);

            return await SendEmailAsync(userEmail, subject, body, isHtmlBody: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send order notification to: {Email}", userEmail);
            return false;
        }
    }

    public async Task<bool> SendNotificationPreferencesEmailAsync(string userEmail, string userName, List<string> selectedMethods)
    {
        try
        {
            var subject = "Notification Preferences Updated";
            var body = BuildNotificationPreferencesHtml(userName, selectedMethods);

            return await SendEmailAsync(userEmail, subject, body, isHtmlBody: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send notification preferences email to: {Email}", userEmail);
            return false;
        }
    }

    private static string BuildOrderNotificationHtml(string userName, string orderId, string orderStatus, decimal totalPrice)
    {
        var html = new StringBuilder();
        html.Append("<html><body style='font-family: Arial, sans-serif;'>");
        html.Append($"<h2>Your Order Status Updated</h2>");
        html.Append($"<p>Dear {userName},</p>");
        html.Append($"<p>Your order <strong>#{orderId}</strong> status has changed to: <strong>{orderStatus}</strong></p>");
        html.Append($"<p>Total Price: <strong>${totalPrice:F2}</strong></p>");
        html.Append($"<p>Updated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>");
        html.Append("<p>Thank you for your business!</p>");
        html.Append("</body></html>");
        return html.ToString();
    }

    private static string BuildNotificationPreferencesHtml(string userName, List<string> selectedMethods)
    {
        var html = new StringBuilder();
        html.Append("<html><body style='font-family: Arial, sans-serif;'>");
        html.Append($"<h2>Notification Preferences Updated</h2>");
        html.Append($"<p>Dear {userName},</p>");
        html.Append($"<p>Your notification preferences have been successfully updated.</p>");
        html.Append($"<p><strong>You will now receive notifications via:</strong></p>");
        html.Append("<ul>");

        foreach (var method in selectedMethods)
        {
            html.Append($"<li>{method}</li>");
        }

        html.Append("</ul>");
        html.Append("<p>You can change these settings at any time in your profile.</p>");
        html.Append("</body></html>");
        return html.ToString();
    }
}