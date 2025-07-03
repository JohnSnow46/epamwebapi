using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.CommentsDto;
public class CommentCreateRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;
}
