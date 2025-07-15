namespace Gamestore.Entities.Orders;

/// <summary>
/// Defines the possible states of an order.
/// </summary>
public enum OrderStatus
{
    Open = 0,

    Checkout = 1,

    Paid = 2,

    Cancelled = 3
}