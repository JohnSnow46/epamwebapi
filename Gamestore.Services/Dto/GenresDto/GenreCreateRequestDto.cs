using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.GenresDto;

/// <summary>
/// Represents a data transfer object for creating a new genre in the game store system.
/// Used to provide information required to create a genre with optional parent-child relationships.
/// </summary>
public class GenreCreateRequestDto
{
    /// <summary>
    /// Gets or sets the name of the genre to be created.
    /// This field is required and should contain the genre name (e.g., "Action", "RPG", "Strategy").
    /// </summary>
    [Required]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the parent genre when creating a sub-genre.
    /// This field is optional and enables hierarchical genre structures (e.g., "Action" -> "First-Person Shooter").
    /// </summary>
    public Guid? ParentGenreId { get; set; }
}
