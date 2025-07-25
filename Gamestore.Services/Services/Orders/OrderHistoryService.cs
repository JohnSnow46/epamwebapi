using Gamestore.Data.Interfaces;
using Gamestore.Services.Dto.OrdersDto;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Gamestore.Services.Services.Orders;

/// <summary>
/// Service for combining order history from both SQL and MongoDB databases
/// Implements E08 US2 requirements
/// </summary>
public class OrderHistoryService : IOrderHistoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderHistoryService> _logger;
    private readonly IMongoCollection<BsonDocument> _mongoOrdersCollection;

    public OrderHistoryService(
        IUnitOfWork unitOfWork,
        ILogger<OrderHistoryService> logger,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;

        try
        {
            var connectionString = configuration.GetConnectionString("MongoDb") ?? "mongodb://localhost:27017";
            var databaseName = configuration.GetValue<string>("MongoDb:DatabaseName") ?? "Northwind";

            _logger.LogInformation("Connecting to MongoDB: {ConnectionString}, Database: {DatabaseName}",
                connectionString, databaseName);

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _mongoOrdersCollection = database.GetCollection<BsonDocument>("orders");

            _logger.LogInformation("MongoDB connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish MongoDB connection");
            _mongoOrdersCollection = null;
        }
    }

    /// <summary>
    /// Gets combined order history from both databases with optional date filtering
    /// </summary>
    public async Task<IEnumerable<object>> GetOrderHistoryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            _logger.LogInformation("Fetching order history - StartDate: {StartDate}, EndDate: {EndDate}",
                startDate, endDate);

            var sqlOrders = await GetSqlOrdersAsync(startDate, endDate);
            _logger.LogInformation("Retrieved {SqlCount} orders from SQL database", sqlOrders.Count());

            var mongoOrders = await GetMongoOrdersAsync(startDate, endDate);
            _logger.LogInformation("Retrieved {MongoCount} orders from MongoDB", mongoOrders.Count());

            var combinedOrders = sqlOrders.Concat(mongoOrders).ToList();

            _logger.LogInformation("Combined total: {TotalCount} orders", combinedOrders.Count);

            return combinedOrders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching combined order history");
            throw;
        }
    }

    /// <summary>
    /// Gets orders from SQL database
    /// </summary>
    private async Task<IEnumerable<object>> GetSqlOrdersAsync(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            _logger.LogDebug("Fetching orders from SQL database");

            var orders = await _unitOfWork.Orders.GetAllAsync();

            if (startDate.HasValue || endDate.HasValue)
            {
                orders = orders.Where(o =>
                {
                    var orderDate = o.Date ?? o.CreatedAt;

                    return (!startDate.HasValue || orderDate >= startDate.Value) && (!endDate.HasValue || orderDate <= endDate.Value);
                });
            }

            var result = orders.Select(o => new
            {
                id = o.Id.ToString(),
                customerId = o.CustomerId.ToString(),
                date = (o.Date ?? o.CreatedAt).ToString("yyyy-MM-ddTHH:mm:ss.fffffffK"),
                source = "SQL"
            }).ToList();

            _logger.LogDebug("Converted {Count} SQL orders to required format", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching SQL orders");
            throw;
        }
    }

    /// <summary>
    /// Gets orders from MongoDB
    /// </summary>
    private async Task<IEnumerable<object>> GetMongoOrdersAsync(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            if (!IsMongoCollectionAvailable())
            {
                return Enumerable.Empty<object>();
            }

            var documents = await FetchMongoDocumentsAsync();
            var orders = ProcessMongoDocuments(documents, startDate, endDate);

            _logger.LogDebug("Converted {Count} MongoDB orders to required format", orders.Count);
            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching MongoDB orders");
            return Enumerable.Empty<object>();
        }
    }

    private bool IsMongoCollectionAvailable()
    {
        if (_mongoOrdersCollection == null)
        {
            _logger.LogWarning("MongoDB collection is not available, skipping MongoDB orders");
            return false;
        }
        return true;
    }

    private async Task<List<BsonDocument>> FetchMongoDocumentsAsync()
    {
        _logger.LogDebug("Fetching orders from MongoDB");
        var documents = await _mongoOrdersCollection.Find(new BsonDocument()).ToListAsync();
        _logger.LogDebug("Retrieved {Count} documents from MongoDB", documents.Count);
        return documents;
    }

    private List<object> ProcessMongoDocuments(List<BsonDocument> documents, DateTime? startDate, DateTime? endDate)
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

                if (IsOrderInDateRange(orderData.OrderDate, startDate, endDate))
                {
                    orders.Add(CreateOrderObject(orderData));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error processing MongoDB document, skipping");
            }
        }

        return orders;
    }

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
        return !orderDate.HasValue || (!startDate.HasValue && !endDate.HasValue) || ((!startDate.HasValue || orderDate.Value >= startDate.Value) && (!endDate.HasValue || orderDate.Value <= endDate.Value));
    }

    private static object CreateOrderObject(OrderData orderData)
    {
        return new
        {
            id = orderData.OrderId,
            customerId = orderData.CustomerId,
            date = orderData.DateString,
            source = "MongoDB"
        };
    }
}