using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.PublishersDto;
public class PublisherUpdateRequestDto
{
    [JsonPropertyName("id")]
    [JsonRequired]
    public Guid Id { get; set; }

    [JsonPropertyName("companyName")]
    [JsonRequired]
    public string CompanyName { get; set; } = string.Empty;

    [JsonPropertyName("homePage")]
    public string? HomePage { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}