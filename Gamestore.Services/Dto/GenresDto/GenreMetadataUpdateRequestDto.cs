using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.GenresDto;
public class GenreMetadataUpdateRequestDto
{
    [JsonPropertyName("id")]
    [JsonRequired]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    [JsonRequired]
    public string? Name { get; set; }

    [JsonPropertyName("parentGenreId")]
    public Guid? ParentGenreId { get; set; }
}
