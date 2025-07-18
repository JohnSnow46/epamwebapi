using Gamestore.Data.Interfaces;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Gamestore.Services.Services;

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
        ILogger<OrderHistoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;

        // Bezpośrednie połączenie z MongoDB (sprawdzone rozwiązanie)
        var client = new MongoClient("mongodb://localhost:27017");
        var database = client.GetDatabase("Northwind");
        _mongoOrdersCollection = database.GetCollection<BsonDocument>("orders");
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

            // 1. Pobierz orders z SQL database
            var sqlOrders = await GetSqlOrdersAsync(startDate, endDate);

            // 2. Pobierz orders z MongoDB
            var mongoOrders = await GetMongoOrdersAsync(startDate, endDate);

            // 3. Połącz bez sortowania
            var combinedOrders = sqlOrders.Concat(mongoOrders).ToList();

            _logger.LogInformation("Combined {SqlCount} SQL orders with {MongoCount} MongoDB orders",
                sqlOrders.Count(), mongoOrders.Count());

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
            // Pobierz wszystkie orders z SQL
            var orders = await _unitOfWork.Orders.GetAllAsync();

            // Filtruj według dat jeśli podane
            if (startDate.HasValue || endDate.HasValue)
            {
                orders = orders.Where(o =>
                {
                    var orderDate = o.Date ?? o.CreatedAt;

                    if (startDate.HasValue && orderDate < startDate.Value)
                        return false;

                    if (endDate.HasValue && orderDate > endDate.Value)
                        return false;

                    return true;
                });
            }

            // Konwertuj do wymaganego formatu
            return orders.Select(o => new
            {
                id = o.Id.ToString(),
                customerId = o.CustomerId.ToString(),
                date = (o.Date ?? o.CreatedAt).ToString("yyyy-MM-ddTHH:mm:ss.fffffffK")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching SQL orders");
            return Enumerable.Empty<object>();
        }
    }

    /// <summary>
    /// Gets orders from MongoDB
    /// </summary>
    private async Task<IEnumerable<object>> GetMongoOrdersAsync(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var documents = await _mongoOrdersCollection.Find(new BsonDocument()).ToListAsync();

            var orders = documents.Select(doc =>
            {
                string dateString;
                DateTime? orderDate = null;

                try
                {
                    if (doc.Contains("OrderDate") && !doc["OrderDate"].IsBsonNull)
                    {
                        var orderDateValue = doc["OrderDate"].AsString;

                        // Parsuj string w formacie: "1996-07-04 00:00:00.000"
                        if (DateTime.TryParse(orderDateValue, out var parsedDate))
                        {
                            orderDate = parsedDate;
                            dateString = parsedDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
                        }
                        else
                        {
                            dateString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
                        }
                    }
                    else
                    {
                        dateString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
                    }
                }
                catch
                {
                    dateString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
                }

                return new
                {
                    order = new
                    {
                        id = doc.Contains("OrderID") ? doc["OrderID"].ToString() : doc["_id"].ToString(),
                        customerId = doc.Contains("CustomerID") ? doc["CustomerID"].AsString : "Unknown",
                        date = dateString
                    },
                    parsedDate = orderDate
                };
            });

            // Filtruj według dat jeśli podane
            if (startDate.HasValue || endDate.HasValue)
            {
                orders = orders.Where(o =>
                {
                    if (!o.parsedDate.HasValue) return false;

                    if (startDate.HasValue && o.parsedDate.Value < startDate.Value)
                        return false;

                    if (endDate.HasValue && o.parsedDate.Value > endDate.Value)
                        return false;

                    return true;
                });
            }

            return orders.Select(o => o.order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching MongoDB orders");
            return Enumerable.Empty<object>();
        }
    }
}