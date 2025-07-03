using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.PlatformsDto;

public class PlatformCreateRequestDto
{
    [JsonIgnore]
    public Guid Id { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;
}
