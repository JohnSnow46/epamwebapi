using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.GenresDto;

public class GenreMetadataUpdateRequestDto
{
    [JsonPropertyName("genre")]
    public GenreUpdateRequestDto Genre { get; set; }
}
