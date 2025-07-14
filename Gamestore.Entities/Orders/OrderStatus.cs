namespace Gamestore.Entities.Orders;

/// <summary>
/// Defines the possible states of an order throughout its lifecycle in the e-commerce system.
/// These status values track order progression from initial cart creation through completion or cancellation,
/// enabling proper order management, workflow control, and business logic implementation.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// The order is in an open state, representing an active shopping cart.
    /// This is the initial status when customers are still adding, removing, or modifying items.
    /// Orders in this state have not been submitted for payment processing.
    /// </summary>
    Open = 0,

    /// <summary>
    /// The order is in checkout state, indicating the customer has initiated the purchase process.
    /// This represents the transition phase where the customer is providing payment information
    /// and finalizing their purchase details before payment processing.
    /// </summary>
    Checkout = 1,

    /// <summary>
    /// The order has been paid and payment processing has completed successfully.
    /// This represents a finalized transaction where payment has been confirmed
    /// and the order is ready for fulfillment or digital delivery.
    /// </summary>
    Paid = 2,

    /// <summary>
    /// The order has been cancelled and will not be fulfilled.
    /// This can occur at any stage of the order lifecycle, either by customer request
    /// or due to system issues, payment failures, or inventory unavailability.
    /// </summary>
    Cancelled = 3
}