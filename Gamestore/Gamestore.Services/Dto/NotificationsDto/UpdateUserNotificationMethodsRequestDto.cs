using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.NotificationsDto;

/// <summary>
/// Request DTO for updating user notification methods.
/// Expected body: { "notifications": ["sms", "email"] }.
/// </summary>
public class UpdateUserNotificationMethodsRequestDto
{
    [Required]
    public List<string> Notifications { get; set; } = new();
}