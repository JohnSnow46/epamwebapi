using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.CommentsDto;

/// <summary>
/// Represents a data transfer object for a comment with full information including hierarchical structure.
/// Used to transfer comment data with support for nested child comments in the game store system.
/// </summary>
public class CommentRequestDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the comment.
    /// This field is required and represents the comment's unique ID in the system.
    /// </summary>
    [Required]
    public Guid Id { get; set; }

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

    /// <summary>
    /// Gets or sets the collection of child comments that are replies to this comment.
    /// This enables hierarchical comment structures with nested replies.
    /// </summary>
    public List<CommentRequestDto> ChildComments { get; set; } = new();
}
