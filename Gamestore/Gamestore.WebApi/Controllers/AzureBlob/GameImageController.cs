using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Gamestore.WebApi.Controllers.AzureBlob;

[ApiController]
[Route("api")]
public class GameImageController(
    IBlobStorageService blobStorageService,
    ILogger<GameImageController> logger) : ControllerBase
{
    private readonly IBlobStorageService _blobStorageService = blobStorageService;
    private readonly ILogger<GameImageController> _logger = logger;

    [HttpGet("games/{key}/image")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGameImage(string key)
    {
        try
        {
            _logger.LogInformation("Getting image for game: {GameKey}", key);

            var blobName = _blobStorageService.GetBlobNameFromGameKey(key);
            var imageBytes = await _blobStorageService.GetImageAsync(blobName);

            if (imageBytes == null)
            {
                _logger.LogWarning("Image not found for game: {GameKey}", key);
                return File(GetPlaceholderImage(), "image/jpeg");
            }

            _logger.LogInformation("Successfully retrieved image for game: {GameKey}", key);
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

    private static byte[] GetPlaceholderImage()
    {
        // Zwróć domyślny obraz (50x50 szary kwadrat lub cokolwiek chcesz)
        using var image = new Image<Rgba32>(200, 200);
        image.Mutate(x => x.BackgroundColor(Color.Gray));

        using var stream = new MemoryStream();
        image.SaveAsJpeg(stream);
        return stream.ToArray();
    }
}