using Gamestore.Data.Interfaces;
using Gamestore.Services.Dto.OrdersDto;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace Gamestore.Services.Services.Orders;

/// <summary>
/// Service for combining order history from both SQL and MongoDB databases
/// REFACTORED: Now follows Single Responsibility Principle
/// - Repository handles MongoDB data access
/// - Service handles business logic, data transformation and combining sources
/// </summary>
public class OrderHistoryService(
    IUnitOfWork unitOfWork,
    IOrderHistoryRepository orderHistoryRepository,
    ILogger<OrderHistoryService> logger) : IOrderHistoryService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IOrderHistoryRepository _orderHistoryRepository = orderHistoryRepository;
    private readonly ILogger<OrderHistoryService> _logger = logger;

    /// <summary>
    /// Gets combined order history from both databases with optional date filtering
    /// Service responsibility: Combine data from multiple sources and transform to business objects
    /// </summary>
    public async Task<IEnumerable<object>> GetOrderHistoryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            _logger.LogInformation("Processing order history request - StartDate: {StartDate}, EndDate: {EndDate}",
                startDate, endDate);

            // Get data from both sources using repositories
            var sqlOrders = await GetSqlOrdersAsync(startDate, endDate);
            _logger.LogInformation("Retrieved {SqlCount} orders from SQL database", sqlOrders.Count());

            var mongoOrders = await GetMongoOrdersAsync(startDate, endDate);
            _logger.LogInformation("Retrieved {MongoCount} orders from MongoDB", mongoOrders.Count());

            // Business logic: combine data from multiple sources
            var combinedOrders = sqlOrders.Concat(mongoOrders).ToList();

            _logger.LogInformation("Combined total: {TotalCount} orders", combinedOrders.Count);
            return combinedOrders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing combined order history request");
            throw;
        }
    }

    /// <summary>
    /// Gets orders from SQL database with transformation
    /// Service responsibility: Transform SQL entities to business objects
    /// </summary>
    private async Task<IEnumerable<object>> GetSqlOrdersAsync(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            _logger.LogDebug("Processing SQL orders request");

            var orders = await _unitOfWork.Orders.GetAllAsync();

            // Business logic: date filtering
            if (startDate.HasValue || endDate.HasValue)
            {
                orders = orders.Where(o =>
                {
                    var orderDate = o.Date ?? o.CreatedAt;
                    return (!startDate.HasValue || orderDate >= startDate.Value) &&
                           (!endDate.HasValue || orderDate <= endDate.Value);
                });
            }

            // Business logic: transform to standardized format
            var result = orders.Select(TransformSqlOrderToBusinessObject).ToList();

            _logger.LogDebug("Transformed {Count} SQL orders to business objects", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SQL orders");
            throw;
        }
    }

    /// <summary>
    /// Gets orders from MongoDB using repository with transformation
    /// Service responsibility: Transform MongoDB documents to business objects
    /// </summary>
    private async Task<IEnumerable<object>> GetMongoOrdersAsync(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            if (!_orderHistoryRepository.IsMongoAvailable())
            {
                _logger.LogWarning("MongoDB is not available, skipping MongoDB orders");
                return Enumerable.Empty<object>();
            }

            // Repository handles data access
            var documents = await _orderHistoryRepository.GetMongoOrdersByDateRangeAsync(startDate, endDate);

            // Service handles business logic: document processing and transformation
            var orders = ProcessMongoDocuments(documents, startDate, endDate);

            _logger.LogDebug("Transformed {Count} MongoDB orders to business objects", orders.Count);
            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MongoDB orders");
            return Enumerable.Empty<object>();
        }
    }

    /// <summary>
    /// Processes MongoDB documents and transforms them to business objects
    /// Service responsibility: Business logic for document processing
    /// </summary>
    private List<object> ProcessMongoDocuments(IEnumerable<BsonDocument> documents, DateTime? startDate, DateTime? endDate)
    {
        var orders = new List<object>();

        foreach (var doc in documents)
        {
            try
            {
                var orderData = ExtractOrderData(doc);
                if (orderData == null)
                {
                    continue;
                }

                // Additional business logic filtering (fallback if repository filtering didn't work)
                if (IsOrderInDateRange(orderData.OrderDate, startDate, endDate))
                {
                    orders.Add(TransformMongoOrderToBusinessObject(orderData));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error processing MongoDB document, skipping");
            }
        }

        return orders;
    }

    // ===================================================================
    // PRIVATE HELPER METHODS - Business Logic
    // ===================================================================

    /// <summary>
    /// Transforms SQL order entity to standardized business object
    /// </summary>
    private object TransformSqlOrderToBusinessObject(dynamic order)
    {
        return new
        {
            id = order.Id.ToString(),
            customerId = order.CustomerId.ToString(),
            date = (order.Date ?? order.CreatedAt).ToString("yyyy-MM-ddTHH:mm:ss.fffffffK"),
            source = "SQL"
        };
    }

    /// <summary>
    /// Transforms MongoDB order data to standardized business object
    /// </summary>
    private static object TransformMongoOrderToBusinessObject(OrderData orderData)
    {
        return new
        {
            id = orderData.OrderId,
            customerId = orderData.CustomerId,
            date = orderData.DateString,
            source = "MongoDB"
        };
    }

    /// <summary>
    /// Extracts order data from MongoDB document
    /// </summary>
    private OrderData? ExtractOrderData(BsonDocument doc)
    {
        var orderId = ExtractOrderId(doc);
        if (orderId == null)
        {
            return null;
        }

        var customerId = ExtractCustomerId(doc);
        var (orderDate, dateString) = ExtractOrderDate(doc, orderId);

        return new OrderData
        {
            OrderId = orderId,
            CustomerId = customerId,
            OrderDate = orderDate,
            DateString = dateString
        };
    }

    private string? ExtractOrderId(BsonDocument doc)
    {
        if (doc.Contains("OrderID"))
        {
            return doc["OrderID"].ToString();
        }

        if (doc.Contains("_id"))
        {
            return doc["_id"].ToString();
        }

        _logger.LogWarning("Document missing OrderID and _id, skipping");
        return null;
    }

    private static string ExtractCustomerId(BsonDocument doc)
    {
        return doc.Contains("CustomerID") && !doc["CustomerID"].IsBsonNull
            ? doc["CustomerID"].AsString
            : "Unknown";
    }

    private (DateTime? orderDate, string dateString) ExtractOrderDate(BsonDocument doc, string orderId)
    {
        if (!doc.Contains("OrderDate") || doc["OrderDate"].IsBsonNull)
        {
            return (null, DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK"));
        }

        var orderDateValue = doc["OrderDate"].AsString;
        if (DateTime.TryParse(orderDateValue, out var parsedDate))
        {
            return (parsedDate, parsedDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK"));
        }

        _logger.LogWarning("Could not parse OrderDate: {OrderDate} for OrderID: {OrderID}",
            orderDateValue, orderId);

        return (null, DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK"));
    }

    private static bool IsOrderInDateRange(DateTime? orderDate, DateTime? startDate, DateTime? endDate)
    {
        return !orderDate.HasValue ||
               (!startDate.HasValue && !endDate.HasValue) ||
               ((!startDate.HasValue || orderDate.Value >= startDate.Value) &&
                (!endDate.HasValue || orderDate.Value <= endDate.Value));
    }
}