using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.AuthDto;
public class UpdateRoleRequest
{
    [Required]
    public RoleUpdateDto Role { get; set; } = new();

    public List<string> Permissions { get; set; } = new();
}
