using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.GenresDto;
public class GenreCreateRequestDto
{
    [Required]
    public string? Name { get; set; }

    public Guid? ParentGenreId { get; set; }
}
