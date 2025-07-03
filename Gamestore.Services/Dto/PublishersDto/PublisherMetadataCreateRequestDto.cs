using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.PublishersDto;
public class PublisherMetadataCreateRequestDto
{
    [JsonIgnore]
    public Guid Id { get; set; }

    [Required]
    public string CompanyName { get; set; } = string.Empty;

    public string? HomePage { get; set; }

    public string? Description { get; set; }
}