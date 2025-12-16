namespace Gamestore.Services.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(string base64Image, string gameKey);

    Task<string?> GetImageUrlAsync(string gameKey);

    Task DeleteImageAsync(string blobName);

    Task<bool> ImageExistsAsync(string blobName);

    string GetBlobNameFromGameKey(string gameKey);
}