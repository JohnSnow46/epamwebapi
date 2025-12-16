namespace Gamestore.Services.Dto.NotificationsDto;

/// <summary>
/// DTO for user's selected notification methods.
/// Used in GET /api/users/my/notifications response.
/// </summary>
public class UserNotificationMethodsDto
{
    public Guid UserId { get; set; }

    public List<string> SelectedMethods { get; set; } = new();

    public List<string> AvailableMethods { get; set; } = new();

    public DateTime UpdatedAt { get; set; }
}