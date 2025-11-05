using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.GenresDto;

public class GenreUpdateRequestDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string? Name { get; set; }

    public Guid? ParentGenreId { get; set; }
}
