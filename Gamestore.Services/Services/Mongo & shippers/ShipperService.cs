using Gamestore.Data.Interfaces;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace Gamestore.Services.Services;

/// <summary>
/// Service for Shipper operations - REFACTORED
/// Now follows Single Responsibility Principle:
/// - Repository handles data access
/// - Service handles business logic and data transformation
/// </summary>
public class ShipperService(
    ILogger<ShipperService> logger,
    IShipperRepository shipperRepository) : IShipperService
{
    private readonly ILogger<ShipperService> _logger = logger;
    private readonly IShipperRepository _shipperRepository = shipperRepository;

    /// <summary>
    /// Gets all shippers with dynamic content structure as per E08 US1
    /// Service responsibility: Transform raw MongoDB data to business objects
    /// </summary>
    public async Task<IEnumerable<object>> GetAllShippersAsync()
    {
        try
        {
            _logger.LogInformation("Processing request for all shippers");

            // Repository handles data access
            var documents = await _shipperRepository.GetAllShippersAsync();

            _logger.LogInformation("Retrieved {Count} shipper documents, transforming to business objects", documents.Count());

            // Service handles business logic - data transformation
            var result = documents.Select(TransformDocumentToBusinessObject);

            _logger.LogInformation("Successfully transformed {Count} shippers", result.Count());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing all shippers request");
            throw;
        }
    }

    /// <summary>
    /// Gets shipper by ID with simplified implementation
    /// Service responsibility: Transform raw MongoDB data to business object
    /// </summary>
    public async Task<object?> GetShipperByIdAsync(int shipperId)
    {
        try
        {
            _logger.LogInformation("Processing request for shipper with ID: {ShipperId}", shipperId);

            // Repository handles data access
            var document = await _shipperRepository.GetShipperByIdAsync(shipperId);

            if (document == null)
            {
                _logger.LogWarning("Shipper with ID {ShipperId} not found", shipperId);
                return null;
            }

            // Service handles business logic - data transformation
            var result = TransformDocumentToBusinessObject(document);

            _logger.LogInformation("Successfully retrieved and transformed shipper {ShipperId}", shipperId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing shipper request for ID {ShipperId}", shipperId);
            throw;
        }
    }

    /// <summary>
    /// Private method for transforming MongoDB document to business object
    /// Centralized transformation logic
    /// </summary>
    private object TransformDocumentToBusinessObject(BsonDocument doc)
    {
        return new
        {
            shipperId = doc.Contains("ShipperID") ? doc["ShipperID"].ToInt32() : 0,
            companyName = doc.Contains("CompanyName") ? doc["CompanyName"].AsString : "N/A",
            phone = doc.Contains("Phone") ? doc["Phone"].AsString : "N/A",
            mongoId = doc["_id"].ToString()
        };
    }
}