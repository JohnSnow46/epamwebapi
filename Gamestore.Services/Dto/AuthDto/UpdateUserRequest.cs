using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.AuthDto;
public class UpdateUserRequest
{
    [Required]
    public UserUpdateDto User { get; set; } = new();

    public string? RoleName { get; set; }
    public string? Password { get; set; }
}
