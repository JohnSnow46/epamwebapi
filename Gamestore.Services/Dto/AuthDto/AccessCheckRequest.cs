using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.AuthDto;
public class AccessCheckRequest
{
    [Required]
    public string TargetPage { get; set; } = string.Empty;

    public string? TargetId { get; set; }
}
