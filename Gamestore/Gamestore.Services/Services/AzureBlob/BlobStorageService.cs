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

    public BlobStorageService(
        IOptions<AzureBlobStorageSettings> settings,
        ILogger<BlobStorageService> logger)
    {
        _settings = settings.Value;
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

            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image for game: {GameKey}", gameKey);
            throw;
        }
    }

    public async Task<byte[]?> GetImageAsync(string blobName)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
            {
                _logger.LogWarning("Image not found: {BlobName}", blobName);
                return null;
            }

            using var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream);

            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving image: {BlobName}", blobName);
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image: {BlobName}", blobName);
            throw;
        }
    }

    public async Task<bool> ImageExistsAsync(string blobName)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        return await blobClient.ExistsAsync();
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
}