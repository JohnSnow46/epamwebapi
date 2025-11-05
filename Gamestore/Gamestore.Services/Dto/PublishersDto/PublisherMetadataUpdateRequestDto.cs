using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.PublishersDto;

public class PublisherMetadataUpdateRequestDto
{
    [JsonPropertyName("publisher")]
    public PublisherUpdateRequestDto Publisher { get; set; } = new();
}