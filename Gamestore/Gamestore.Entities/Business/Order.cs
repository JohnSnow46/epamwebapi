using Gamestore.Entities.Notifications;

namespace Gamestore.Entities.Business;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string CustomerId { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public DateTime? ShippedDate { get; set; }

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    // Epic 12 - Order Notifications
    public ICollection<OrderNotification> OrderNotifications { get; set; } = new List<OrderNotification>();
}
