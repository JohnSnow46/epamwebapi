using Gamestore.Data.Interfaces;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Configuration;

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
        ILogger<OrderHistoryService> logger,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;

        try
        {
            // Pobierz connection string z konfiguracji lub użyj domyślny
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
            // W przypadku błędu, utwórz null collection - zostanie obsłużone w metodach
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

            // 1. Pobierz orders z SQL database
            var sqlOrders = await GetSqlOrdersAsync(startDate, endDate);
            _logger.LogInformation("Retrieved {SqlCount} orders from SQL database", sqlOrders.Count());

            // 2. Pobierz orders z MongoDB (jeśli dostępny)
            var mongoOrders = await GetMongoOrdersAsync(startDate, endDate);
            _logger.LogInformation("Retrieved {MongoCount} orders from MongoDB", mongoOrders.Count());

            // 3. Połącz wszystkie orders
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

            // Konwertuj do wymaganego formatu - zgodnego z tym co oczekuje UI
            var result = orders.Select(o => new
            {
                id = o.Id.ToString(),
                customerId = o.CustomerId.ToString(),
                date = (o.Date ?? o.CreatedAt).ToString("yyyy-MM-ddTHH:mm:ss.fffffffK"),
                source = "SQL" // Dodatkowe pole dla debugowania
            }).ToList();

            _logger.LogDebug("Converted {Count} SQL orders to required format", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching SQL orders");
            throw; // Rzuć wyjątek dalej zamiast zwracać pustą kolekcję
        }
    }

    /// <summary>
    /// Gets orders from MongoDB
    /// </summary>
    private async Task<IEnumerable<object>> GetMongoOrdersAsync(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            // Jeśli MongoDB nie jest dostępny, zwróć pustą kolekcję
            if (_mongoOrdersCollection == null)
            {
                _logger.LogWarning("MongoDB collection is not available, skipping MongoDB orders");
                return Enumerable.Empty<object>();
            }

            _logger.LogDebug("Fetching orders from MongoDB");

            // Pobierz wszystkie dokumenty z MongoDB
            var documents = await _mongoOrdersCollection.Find(new BsonDocument()).ToListAsync();
            _logger.LogDebug("Retrieved {Count} documents from MongoDB", documents.Count);

            var orders = new List<object>();

            foreach (var doc in documents)
            {
                try
                {
                    string orderId;
                    string customerId;
                    DateTime? orderDate = null;
                    string dateString;

                    // Pobierz OrderID
                    if (doc.Contains("OrderID"))
                    {
                        orderId = doc["OrderID"].ToString();
                    }
                    else if (doc.Contains("_id"))
                    {
                        orderId = doc["_id"].ToString();
                    }
                    else
                    {
                        _logger.LogWarning("Document missing OrderID and _id, skipping");
                        continue;
                    }

                    // Pobierz CustomerID
                    if (doc.Contains("CustomerID") && !doc["CustomerID"].IsBsonNull)
                    {
                        customerId = doc["CustomerID"].AsString;
                    }
                    else
                    {
                        customerId = "Unknown";
                    }

                    // Pobierz datę
                    if (doc.Contains("OrderDate") && !doc["OrderDate"].IsBsonNull)
                    {
                        var orderDateValue = doc["OrderDate"].AsString;

                        if (DateTime.TryParse(orderDateValue, out var parsedDate))
                        {
                            orderDate = parsedDate;
                            dateString = parsedDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
                        }
                        else
                        {
                            _logger.LogWarning("Could not parse OrderDate: {OrderDate} for OrderID: {OrderID}",
                                orderDateValue, orderId);
                            dateString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
                        }
                    }
                    else
                    {
                        dateString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
                    }

                    // Filtruj według dat jeśli podane
                    if ((startDate.HasValue || endDate.HasValue) && orderDate.HasValue)
                    {
                        if (startDate.HasValue && orderDate.Value < startDate.Value)
                            continue;

                        if (endDate.HasValue && orderDate.Value > endDate.Value)
                            continue;
                    }

                    // POPRAWKA: Zwróć strukturę zgodną z SQL (bez zagnieżdżenia "order")
                    orders.Add(new
                    {
                        id = orderId,
                        customerId = customerId,
                        date = dateString,
                        source = "MongoDB" // Dodatkowe pole dla debugowania
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing MongoDB document, skipping");
                    continue;
                }
            }

            _logger.LogDebug("Converted {Count} MongoDB orders to required format", orders.Count);
            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching MongoDB orders");
            // W przypadku błędu MongoDB, zwróć pustą kolekcję ale zloguj błąd
            return Enumerable.Empty<object>();
        }
    }
}