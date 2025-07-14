using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.CommentsDto;

/// <summary>
/// Represents a data transfer object for creating a new comment in the game store system.
/// Used to provide information required to create a comment on games, reviews, or other content.
/// </summary>
public class CommentCreateRequestDto
{
    /// <summary>
    /// Gets or sets the name or title of the comment.
    /// This field is required and cannot be empty.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content body of the comment.
    /// This field is required and contains the actual comment text.
    /// </summary>
    [Required]
    public string Body { get; set; } = string.Empty;
}
