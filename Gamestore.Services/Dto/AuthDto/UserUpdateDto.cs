using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.AuthDto;
public class UserUpdateDto
{
    [EmailAddress]
    public string Email { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
}
