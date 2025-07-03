using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.PlatformsDto;
public class PlatformMetadataUpdateRequestDto
{
    [JsonPropertyName("id")]
    [JsonRequired]
    public Guid Id { get; set; }

    [JsonPropertyName("type")]
    [JsonRequired]
    public string Type { get; set; } = string.Empty;
}
