using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.CommentsDto;
public class CommentMetadataRequestDto
{
    [Required]
    public CommentCreateRequestDto Comment { get; set; } = new();

    public string? ParentId { get; set; }

    public string? Action { get; set; }
}
