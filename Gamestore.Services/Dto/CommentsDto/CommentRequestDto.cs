using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.CommentsDto;
public class CommentRequestDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public List<CommentRequestDto> ChildComments { get; set; } = new();
}
