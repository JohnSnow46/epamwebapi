namespace Gamestore.Services.Dto.OrdersDto;

public class ShipOrderResponse
{
    public Guid OrderId { get; set; }

    public DateTime ShippedDate { get; set; }

    public bool NotificationsSent { get; set; }

    public int NotificationsCount { get; set; }

    public List<string> RecipientEmails { get; set; } = new();
}