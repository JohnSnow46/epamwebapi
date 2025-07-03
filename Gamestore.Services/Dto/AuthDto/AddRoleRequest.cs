using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.AuthDto;
public class AddRoleRequest
{
    [Required]
    public RoleCreateDto Role { get; set; } = new();

    public List<string> Permissions { get; set; }
}