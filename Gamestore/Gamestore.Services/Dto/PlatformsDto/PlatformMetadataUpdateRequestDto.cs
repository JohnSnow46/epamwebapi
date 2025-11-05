using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.PlatformsDto;

public class PlatformMetadataUpdateRequestDto
{
    [JsonPropertyName("platform")]
    public PlatformUpdateRequestDto Platform { get; set; }
}
