using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.GamesDto;
public class GameMetadataUpdateRequestDto
{
    [JsonPropertyName("game")]
    public GameUpdateRequestDto Game { get; set; } = new();

    [JsonPropertyName("publisher")]
    [JsonRequired]
    public Guid Publisher { get; set; }

    [JsonPropertyName("genres")]
    public List<Guid>? Genres { get; set; }

    [JsonPropertyName("platforms")]
    public List<Guid>? Platforms { get; set; }
}
