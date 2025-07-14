namespace Gamestore.Entities.Orders;

/// <summary>
/// Defines the possible states of a payment transaction throughout its processing lifecycle.
/// These status values track payment progression from initiation through completion or failure,
/// enabling proper payment monitoring, error handling, and transaction management.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// The payment transaction has been initiated but not yet processed.
    /// This is the initial status when a payment request has been created
    /// but processing has not begun or is awaiting external validation.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// The payment transaction is currently being processed by the payment system.
    /// This indicates that the payment gateway or processor is actively working
    /// on validating and completing the transaction.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// The payment transaction has been successfully completed and funds have been transferred.
    /// This represents a successful payment where all validations passed
    /// and the transaction has been finalized.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// The payment transaction has failed due to an error or rejection.
    /// This can occur due to insufficient funds, invalid payment information,
    /// technical issues, or security validations failing.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// The payment transaction has been cancelled before completion.
    /// This can happen due to user cancellation, timeout, system issues,
    /// or business logic preventing the transaction from proceeding.
    /// </summary>
    Cancelled = 4
}