namespace Gamestore.Services.Interfaces;

/// <summary>
/// Service interface for Order History operations
/// E08 US2 - Combines orders from both SQL and MongoDB databases
/// </summary>
public interface IOrderHistoryService
{
    /// <summary>
    /// Gets combined order history from both databases
    /// </summary>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>Combined order history from SQL and MongoDB</returns>
    Task<IEnumerable<object>> GetOrderHistoryAsync(DateTime? startDate = null, DateTime? endDate = null);
}