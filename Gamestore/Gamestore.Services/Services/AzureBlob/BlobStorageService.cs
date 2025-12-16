using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Gamestore.Services.Configuration;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Gamestore.Services.Services.AzureBlob;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<BlobStorageService> _logger;
    private readonly AzureBlobStorageSettings _settings;
    private readonly ICacheService _cacheService;

    public BlobStorageService(
        IOptions<AzureBlobStorageSettings> settings,
        ICacheService cacheService,
        ILogger<BlobStorageService> logger)
    {
        _settings = settings.Value;
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger;

        var blobServiceClient = new BlobServiceClient(_settings.ConnectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(_settings.ContainerName);

        _containerClient.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string> UploadImageAsync(string base64Image, string gameKey)
    {
        try
        {
            var base64Data = base64Image.Contains(',')
                ? base64Image.Split(',')[1]
                : base64Image;

            var imageBytes = Convert.FromBase64String(base64Data);

            var optimizedImage = await OptimizeImageAsync(imageBytes);

            var blobName = GetBlobNameFromGameKey(gameKey);
            var blobClient = _containerClient.GetBlobClient(blobName);

            bool isUpdate = await blobClient.ExistsAsync();

            using var stream = new MemoryStream(optimizedImage);

            await blobClient.UploadAsync(
                stream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = "image/jpeg",
                    },
                    Conditions = null,
                });

            _logger.LogInformation("Successfully uploaded image for game: {GameKey}", gameKey);

            InvalidateImageCacheForGameKey(gameKey);

            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image for game: {GameKey}", gameKey);
            throw;
        }
    }

    public async Task<string?> GetImageUrlAsync(string gameKey)
    {
        try
        {
            var cacheKey = GetImageUrlCache(gameKey);

            if (_cacheService.TryGetValue<string>(cacheKey, out var cachedUrl))
            {
                _logger.LogDebug("Retrieved image URL from cache: {GameKey}", gameKey);
                return cachedUrl;
            }

            var blobName = GetBlobNameFromGameKey(gameKey);
            var blobClient = _containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
            {
                _logger.LogWarning("Image not found for game: {GameKey}", gameKey);
                return null;
            }

            var url = blobClient.Uri.ToString();

            _cacheService.Set(cacheKey, url, TimeSpan.FromMinutes(60));

            _logger.LogInformation("Retrieved and cached image URL: {GameKey}", gameKey);

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting image URL: {GameKey}", gameKey);
            throw;
        }
    }

    public async Task DeleteImageAsync(string blobName)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();

            _logger.LogInformation("Successfully deleted image: {BlobName}", blobName);

            var gameKey = Path.GetFileNameWithoutExtension(blobName);

            InvalidateImageCacheForGameKey(gameKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image: {BlobName}", blobName);
            throw;
        }
    }

    public async Task<bool> ImageExistsAsync(string blobName)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            return await blobClient.ExistsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if image exists: {BlobName}", blobName);
            return false;
        }
    }

    public string GetBlobNameFromGameKey(string gameKey)
    {
        return $"{gameKey.ToLower()}.jpg";
    }

    public void ClearImageCache(string gameKey, IMemoryCache memoryCache)
    {
        try
        {
            var cacheKey = $"game-image-{gameKey}";
            memoryCache.Remove(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache for game: {GameKey}", gameKey);
        }
    }

    private static async Task<byte[]> OptimizeImageAsync(byte[] imageBytes)
    {
        using var image = await Image.LoadAsync(new MemoryStream(imageBytes));

        if (image.Width > 1920 || image.Height > 1080)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(1920, 1080),
                Mode = ResizeMode.Max,
            }));
        }

        using var outputStream = new MemoryStream();
        await image.SaveAsJpegAsync(outputStream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
        {
            Quality = 85,
        });

        return outputStream.ToArray();
    }

    private void InvalidateImageCacheForGameKey(string gameKey)
    {
        try
        {
            var blobName = GetBlobNameFromGameKey(gameKey);

            // Remove all image-related caches
            _cacheService.RemoveMultiple(
                GetImageUrlCache(gameKey),
                GetImageBytesCache(blobName));

            _logger.LogInformation(
                "Invalidated image caches for game: {GameKey}",
                gameKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error invalidating image cache for game: {GameKey}. Continuing anyway.",
                gameKey);
        }
    }

    private static string GetImageUrlCache(string gameKey)
    {
        return $"image_url_{gameKey.ToLower()}";
    }

    private static string GetImageBytesCache(string blobName)
    {
        return $"image_bytes_{blobName.ToLower()}";
    }
}