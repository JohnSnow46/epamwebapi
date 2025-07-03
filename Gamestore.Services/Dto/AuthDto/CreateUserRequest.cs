using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.AuthDto;
public class CreateUserRequest
{
    [Required]
    public UserCreateDto User { get; set; } = new();

    [Required]
    public string Password { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = new();
}
