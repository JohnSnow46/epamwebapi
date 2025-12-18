using Gamestore.Entities.Business;

namespace Gamestore.Entities.Notifications;

public class OrderNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public string RecipientEmail { get; set; } = string.Empty;

    public string NotificationType { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
