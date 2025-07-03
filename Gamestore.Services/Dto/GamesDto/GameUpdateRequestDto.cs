using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.GamesDto;
public class GameUpdateRequestDto
{
    [JsonPropertyName("id")]
    [JsonRequired]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    [JsonRequired]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("key")]
    [JsonRequired]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    [JsonRequired]
    public string? Description { get; set; }

    [JsonPropertyName("price")]
    [JsonRequired]
    public double Price { get; set; }

    [JsonPropertyName("unitInStock")]
    [JsonRequired]
    public int UnitInStock { get; set; }

    [JsonPropertyName("discount")]
    [JsonRequired]
    public int Discontinued { get; set; }
}
