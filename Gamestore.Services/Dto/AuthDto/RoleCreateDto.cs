using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.AuthDto;
public class RoleCreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
}
