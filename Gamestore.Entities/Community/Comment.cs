using Gamestore.Entities.Business;

namespace Gamestore.Entities.Community;

/// <summary>
/// Represents a comment entity in the community discussion system.
/// Supports hierarchical comment structures with parent-child relationships,
/// enabling threaded discussions, reviews, and nested replies for comprehensive user engagement.
/// </summary>
public class Comment
{
    /// <summary>
    /// Gets or sets the unique identifier for the comment.
    /// This serves as the primary key for the comment entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name or identifier of the comment author.
    /// This could be a username, display name, or anonymous identifier
    /// for the person who posted the comment.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the main content body of the comment.
    /// This contains the actual text content of the user's comment, review, or discussion post.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier of the parent comment in the hierarchy.
    /// This is nullable to support root-level comments that are not replies to other comments.
    /// Enables threaded discussion structures where comments can be replies to other comments.
    /// </summary>
    public Guid? ParentCommentId { get; set; }

    /// <summary>
    /// Gets or sets the parent Comment entity in the hierarchical structure.
    /// This navigation property provides access to the comment being replied to,
    /// enabling the building of comment trees and reply chains.
    /// Can be null for top-level comments that are not replies.
    /// </summary>
    public Comment? ParentComment { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the game this comment is associated with.
    /// This is nullable to support comments that may not be tied to specific games,
    /// such as general forum discussions or system-wide announcements.
    /// </summary>
    public Guid? GameId { get; set; }

    /// <summary>
    /// Gets or sets the Game entity that this comment is associated with.
    /// This navigation property provides access to the complete game information
    /// that the comment is discussing or reviewing.
    /// </summary>
    public Game Game { get; set; }

    /// <summary>
    /// Gets or sets the collection of child comments that are replies to this comment.
    /// This navigation property enables access to nested replies and supports
    /// building complete comment threads and discussion trees.
    /// Can be null or empty for comments with no replies.
    /// </summary>
    public ICollection<Comment>? ChildComments { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this comment has been deleted.
    /// Deleted comments are typically hidden from display but preserved for audit purposes.
    /// This enables soft deletion to maintain discussion thread integrity while removing inappropriate content.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the comment was created.
    /// This is automatically set to the current UTC time when the comment is instantiated,
    /// used for chronological ordering and display of discussion timelines.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}