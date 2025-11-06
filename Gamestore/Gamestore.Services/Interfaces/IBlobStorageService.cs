namespace Gamestore.Services.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(string base64Image, string gameKey);

    Task<byte[]?> GetImageAsync(string blobName);

    Task DeleteImageAsync(string blobName);

    Task<bool> ImageExistsAsync(string blobName);

    string GetBlobNameFromGameKey(string gameKey);

    void ClearImageCache(string gameKey, Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache);
}