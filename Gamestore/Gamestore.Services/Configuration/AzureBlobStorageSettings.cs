namespace Gamestore.Services.Configuration;

public class AzureBlobStorageSettings
{
    public string ConnectionString { get; set; } = string.Empty;

    public string ContainerName { get; set; } = "game-images";

    public int CacheExpirationMinutes { get; set; } = 60;
}