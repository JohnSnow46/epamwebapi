using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.PublishersDto;

public class PublisherMetadataCreateRequestDto
{
    [JsonPropertyName("publisher")]
    public PublisherCreateRequestDto Publisher { get; set; } = new();
}