using Gamestore.Entities.Business;
using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Dto.PublishersDto;
using Gamestore.Services.Interfaces;
using Gamestore.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.Business;

[Route("api/publishers")]
[ApiController]
public class PublishersController(IPublisherService publisherService, ILogger<PublishersController> logger) : ControllerBase
{
    private readonly IPublisherService _publisherService = publisherService;
    private readonly ILogger<PublishersController> _logger = logger;

    /// <summary>
    /// Get all publishers
    /// GET /api/publishers.
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
    /// Get publisher by company name
    /// GET /api/publishers/{companyname}.
    /// </summary>
    [HttpGet("{companyname}")]
    [AllowAnonymous]
    public async Task<ActionResult<Publisher>> GetPublisherByName(string companyname)
    {
        try
        {
            _logger.LogInformation("Getting publisher by Name: {PublisherName}", companyname);
            var publisher = await _publisherService.GetPublisherByCompanyNameAsync(companyname);

            if (publisher == null)
            {
                return ResourceNotFound($"Publisher with name '{companyname}' not found.");
            }

            _logger.LogInformation("Successfully found Publisher with name: {PublisherName}", companyname);
            return Ok(publisher);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error getting publisher by name: {companyname}");
        }
    }

    /// <summary>
    /// Get games by publisher company name
    /// GET /api/publishers/{companyname}/games.
    /// </summary>
    [HttpGet("{companyname}/games")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Game>>> GetGamesByPublisher(string companyname)
    {
        try
        {
            _logger.LogInformation("Getting games for publisher: {PublisherName}", companyname);
            var games = await _publisherService.GetGamesByPublisherNameAsync(companyname);

            if (games == null || !games.Any())
            {
                return ResourceNotFound($"No games found for publisher '{companyname}'.");
            }

            _logger.LogInformation("Successfully retrieved {Count} games for publisher: {PublisherName}", games.Count(), companyname);
            return Ok(games);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error getting games for publisher: {companyname}");
        }
    }

    /// <summary>
    /// Create new publisher
    /// POST /api/publishers.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "CanManageBusinessEntities")]
    public async Task<IActionResult> CreatePublisher([FromBody] PublisherCreateRequestDto publisherRequest)
    {
        try
        {
            if (publisherRequest == null)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Publisher data is required.",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }

            _logger.LogInformation(
                "Creating publisher with Name: {PublisherName} by user: {User}",
                publisherRequest.CompanyName,
                User.GetUserEmail());

            var createdPublisher = await _publisherService.AddPublisherAsync(publisherRequest);

            _logger.LogInformation(
                "Successfully created publisher with Name: {PublisherName}",
                publisherRequest.CompanyName);

            return Ok(createdPublisher);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error creating publisher");
        }
    }

    /// <summary>
    /// Update publisher
    /// PUT /api/publishers.
    /// </summary>
    [HttpPut]
    [Authorize(Policy = "CanManageBusinessEntities")]
    public async Task<IActionResult> UpdatePublisher([FromBody] PublisherMetadataUpdateRequestDto publisherUpdateDto)
    {
        try
        {
            if (publisherUpdateDto?.Publisher == null || publisherUpdateDto.Publisher.Id == Guid.Empty)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Invalid publisher data or missing ID.",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }

            _logger.LogInformation(
                "Received publisher update request for ID: {PublisherId} from user: {User}",
                publisherUpdateDto.Publisher.Id,
                User.GetUserEmail());

            var id = publisherUpdateDto.Publisher.Id;
            var updatedPublisher = await _publisherService.UpdatePublisherAsync(id, publisherUpdateDto.Publisher);

            _logger.LogInformation("Successfully updated publisher with ID: {PublisherId}", updatedPublisher.Id);
            return Ok(updatedPublisher);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error updating publisher");
        }
    }

    /// <summary>
    /// Delete publisher
    /// DELETE /api/publishers/{id}.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanManageBusinessEntities")]
    public async Task<IActionResult> DeletePublisherById(Guid id)
    {
        try
        {
            _logger.LogInformation(
                "Deleting publisher with ID: {Id} by user: {User}",
                id,
                User.GetUserEmail());

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