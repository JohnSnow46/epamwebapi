namespace Gamestore.Services.Dto.GenresDto;

/// <summary>
/// Represents a data transfer object for creating a genre with metadata information.
/// Used to provide both genre creation data and additional metadata context in the game store system.
/// </summary>
public class GenreMetadataCreateRequestDto
{
    /// <summary>
    /// Gets or sets the genre creation request containing the main genre information.
    /// This includes the genre name and optional parent genre identifier.
    /// </summary>
    public GenreCreateRequestDto Genre { get; set; }
}
