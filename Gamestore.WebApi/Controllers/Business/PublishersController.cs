using System.Text.Json;
using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Dto.PublishersDto;
using Gamestore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Gamestore.WebApi.Extensions;
using Gamestore.Entities.Business;

namespace Gamestore.WebApi.Controllers.Business;

[Route("api/publishers")]
[ApiController]
public class PublishersController(IPublisherService publisherService, ILogger<PublishersController> logger) : ControllerBase
{
    private readonly IPublisherService _publisherService = publisherService;
    private readonly ILogger<PublishersController> _logger = logger;

    /// <summary>
    /// Epic 9: Everyone can view publisher by name
    /// </summary>
    [HttpGet("name/{companyName}")]
    [AllowAnonymous]
    public async Task<ActionResult<Publisher>> GetPublisherByName(string companyName)
    {
        try
        {
            _logger.LogInformation("Getting publisher by Name: {PublisherName}", companyName);
            var publisher = await _publisherService.GetPublisherByCompanyNameAsync(companyName);

            if (publisher == null)
            {
                return ResourceNotFound($"Publisher with name '{companyName}' not found.");
            }

            _logger.LogInformation("Successfully found Publisher with name: {PublisherName}", companyName);
            return Ok(publisher);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error getting publisher by name: {companyName}");
        }
    }

    /// <summary>
    /// Epic 9: Everyone can view all publishers
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Publisher>>> GetAllPublishers()
    {
        try
        {
            _logger.LogInformation("Getting all publishers");
            var publishers = await _publisherService.GetAllPublishersAsync();

            if (publishers == null || !publishers.Any())
            {
                return ResourceNotFound("No publishers found.");
            }

            _logger.LogInformation("Successfully retrieved {Count} publishers", publishers.Count());
            return Ok(publishers);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving all publishers");
        }
    }

    /// <summary>
    /// Epic 9: Everyone can view publisher by game key
    /// </summary>
    [HttpGet("game/{key}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGamePublisherByGameKey(string key)
    {
        try
        {
            _logger.LogInformation("Getting publisher for game with key: {Key}", key);
            var publisher = await _publisherService.GetPublisherByGameKey(key);

            if (publisher == null)
            {
                return ResourceNotFound($"Publisher for game with key: '{key}' not found.");
            }

            _logger.LogInformation("Successfully found publisher for game with key: {Key}", key);
            return Ok(publisher);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error getting publisher for game with key: {key}");
        }
    }

    /// <summary>
    /// Epic 9: Admin and Manager can create publishers
    /// </summary>
    [HttpPost("add-publisher")]
    [Authorize(Policy = "CanManageBusinessEntities")]
    public async Task<IActionResult> CreatePublisher([FromBody] PublisherCreateRequestDto publisherRequest)
    {
        try
        {
            _logger.LogInformation("Creating publisher with Name: {PublisherName} by user: {User}",
                publisherRequest.Publisher.CompanyName, User.GetUserEmail());

            if (publisherRequest == null)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Publisher data is required.",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }

            var publisher = await _publisherService.AddPublisherAsync(publisherRequest);

            _logger.LogInformation("Successfully created publisher with Name: {PublisherName}", publisher.Publisher.CompanyName);
            return Ok(publisher);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error creating publisher");
        }
    }

    /// <summary>
    /// Epic 9: Admin and Manager can update publishers
    /// </summary>
    [HttpPut("update-publisher")]
    [Authorize(Policy = "CanManageBusinessEntities")]
    public async Task<IActionResult> UpdatePublisher([FromBody] JsonElement requestData)
    {
        try
        {
            _logger.LogInformation("Received publisher update request from user: {User}", User.GetUserEmail());

            if (!requestData.TryGetProperty("publisher", out var publisherElement))
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Invalid request format. Expected 'publisher' property.",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }

            var publisherUpdate = publisherElement.Deserialize<PublisherUpdateRequestDto>();
            if (publisherUpdate == null || publisherUpdate.Id == Guid.Empty)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Invalid publisher data or missing ID.",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }

            var id = publisherUpdate.Id;
            _logger.LogInformation("Updating publisher with ID: {PublisherId}", id);

            var updatedPublisher = await _publisherService.UpdatePublisherAsync(id, publisherUpdate);

            _logger.LogInformation("Successfully updated publisher with ID: {PublisherId}", updatedPublisher.Id);
            return Ok(new { publisher = updatedPublisher });
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error updating publisher");
        }
    }

    /// <summary>
    /// Epic 9: Admin and Manager can delete publishers
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanManageBusinessEntities")]
    public async Task<IActionResult> DeletePublisherById(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting publisher with ID: {Id} by user: {User}", id, User.GetUserEmail());
            var deletedPublisher = await _publisherService.DeletePublisherAsync(id);

            if (deletedPublisher == null)
            {
                return ResourceNotFound($"Publisher with ID '{id}' not found.");
            }

            _logger.LogInformation("Successfully deleted publisher with ID: {Id}", id);
            return Ok(deletedPublisher);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error deleting publisher with ID: {id}");
        }
    }

    private NotFoundObjectResult ResourceNotFound(string message)
    {
        _logger.LogWarning(message);

        return NotFound(new ErrorResponseModel
        {
            Message = message,
            StatusCode = StatusCodes.Status404NotFound,
        });
    }

    private ObjectResult HandleException(Exception ex, string logMessage)
    {
        _logger.LogError(ex, "{LogMessage}: {ErrorMessage}", logMessage, ex.Message);

        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseModel
        {
            Message = "An error occurred.",
            Details = ex.Message,
            StatusCode = StatusCodes.Status500InternalServerError,
        });
    }
}