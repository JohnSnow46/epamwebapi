using Gamestore.Entities.Business;

namespace Gamestore.Entities.Community;
public class Comment
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Body { get; set; } = string.Empty;

    public Guid? ParentCommentId { get; set; }

    public Comment? ParentComment { get; set; }

    public Guid? GameId { get; set; }

    public Game Game { get; set; }

    public ICollection<Comment>? ChildComments { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
