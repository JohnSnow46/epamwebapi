using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.GenresDto;

/// <summary>
/// Represents a data transfer object for updating an existing genre in the game store system.
/// Used to modify genre properties including name and parent-child relationships with validation requirements.
/// </summary>
public class GenreUpdateRequestDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the genre to be updated.
    /// This field is required and must match an existing genre in the system.
    /// </summary>
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the new name for the genre.
    /// This field is required and should contain the updated genre name.
    /// </summary>
    [Required]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the parent genre when updating hierarchical relationships.
    /// This field is required and can be used to change the genre's parent or establish new hierarchies.
    /// </summary>
    [Required]
    public Guid? ParentGenreId { get; set; }
}
