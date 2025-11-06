using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Gamestore.WebApi.Controllers.AzureBlob;

[ApiController]
[Route("api")]
public class GameImageController(
    IBlobStorageService blobStorageService,
    IMemoryCache memoryCache,
    ILogger<GameImageController> logger) : ControllerBase
{
    private readonly IBlobStorageService _blobStorageService = blobStorageService;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly ILogger<GameImageController> _logger = logger;

    [HttpGet("games/{key}/image")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGameImage(string key)
    {
        try
        {
            var cacheKey = $"game-image-{key}";

#nullable enable
            if (_memoryCache.TryGetValue(cacheKey, out byte[]? cachedImage) && cachedImage != null)
            {
                _logger.LogInformation("Returning cached image for game: {GameKey}", key);

                var etag = $"\"{key}-{DateTime.UtcNow:yyyy-MM-dd-HH}\"";
                Response.Headers.ETag = etag;
                Response.Headers.CacheControl = "public, max-age=300";

                return File(cachedImage, "image/jpeg");
            }
#nullable disable

            var blobName = _blobStorageService.GetBlobNameFromGameKey(key);
            var imageBytes = await _blobStorageService.GetImageAsync(blobName);

            if (imageBytes == null)
            {
                _logger.LogWarning("Image not found for game: {GameKey}", key);
                return NotFound(new ErrorResponseModel
                {
                    Message = $"Image not found for game '{key}'",
                    StatusCode = StatusCodes.Status404NotFound,
                });
            }

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetPriority(CacheItemPriority.Normal);

            _memoryCache.Set(cacheKey, imageBytes, cacheOptions);

            _logger.LogInformation("Successfully retrieved and cached image for game: {GameKey}", key);

            var etagNew = $"\"{key}-{DateTime.UtcNow:yyyy-MM-dd-HH}\"";
            Response.Headers.ETag = etagNew;
            Response.Headers.CacheControl = "public, max-age=300";

            return File(imageBytes, "image/jpeg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving image for game: {GameKey}", key);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseModel
            {
                Message = "An error occurred while retrieving the game image",
                StatusCode = StatusCodes.Status500InternalServerError,
            });
        }
    }
}