using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.CommentsDto;
public class BanCreateRequestDto
{
    [Required]
    public string User { get; set; } = string.Empty;

    [Required]
    public string Duration { get; set; } = string.Empty;
}
