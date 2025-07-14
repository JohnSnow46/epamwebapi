using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.CommentsDto;

/// <summary>
/// Represents a data transfer object for creating a comment with additional metadata information.
/// Used to provide both comment content and contextual information such as parent comment relationships and actions.
/// </summary>
public class CommentMetadataRequestDto
{
    /// <summary>
    /// Gets or sets the comment creation request containing the main comment information.
    /// This field is required and contains the name and body of the comment.
    /// </summary>
    [Required]
    public CommentCreateRequestDto Comment { get; set; } = new();

    /// <summary>
    /// Gets or sets the identifier of the parent comment when creating a reply.
    /// This field is optional and should be specified when creating a nested comment or reply.
    /// </summary>
    public string? ParentId { get; set; }

    /// <summary>
    /// Gets or sets the action type associated with this comment request.
    /// This field is optional and can specify the type of action being performed (e.g., "create", "reply", "edit").
    /// </summary>
    public string? Action { get; set; }
}
