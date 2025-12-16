using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.AzureBlob;

[ApiController]
[Route("api")]
public class GameImageController(
    IBlobStorageService blobStorageService,
    ILogger<GameImageController> logger) : ControllerBase
{
    private readonly IBlobStorageService _blobStorageService = blobStorageService;
    private readonly ILogger<GameImageController> _logger = logger;

    [HttpGet("{gameKey}/url")]
    [AllowAnonymous]
    public async Task<ActionResult<ImageUrlResponse>> GetImageUrl(string gameKey)
    {
        try
        {
            _logger.LogDebug("Requesting image URL for game: {GameKey}", gameKey);

            var url = await _blobStorageService.GetImageUrlAsync(gameKey);

            if (url == null)
            {
                _logger.LogWarning("Image not found for game: {GameKey}", gameKey);
                return NotFound(new { message = "Image not found for this game" });
            }

            // Client-side cache for 10 minutes
            Response.Headers.CacheControl = "public, max-age=600";

            return Ok(new ImageUrlResponse { Url = url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving image URL for game: {GameKey}", gameKey);
            return StatusCode(500, new { message = "Error retrieving image URL" });
        }
    }

    public class ImageUrlResponse
    {
        /// <summary>
        /// Public URL to the game image in Azure Blob Storage.
        /// </summary>
        public string Url { get; set; } = string.Empty;
    }
}