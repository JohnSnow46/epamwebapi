using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.AuthDto;
public class UserCreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
}