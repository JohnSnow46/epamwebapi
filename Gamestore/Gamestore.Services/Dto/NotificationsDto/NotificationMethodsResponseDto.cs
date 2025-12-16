namespace Gamestore.Services.Dto.NotificationsDto;

/// <summary>
/// Response DTO for available notification methods.
/// Simple list of strings.
/// </summary>
public class NotificationMethodsResponseDto
{
    public List<string> Methods { get; set; } = new();
}