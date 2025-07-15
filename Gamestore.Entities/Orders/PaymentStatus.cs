namespace Gamestore.Entities.Orders;

/// <summary>
/// Defines the possible states of a payment.
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}