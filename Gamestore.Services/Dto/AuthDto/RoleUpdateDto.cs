using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.AuthDto;
public class RoleUpdateDto
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;
}
