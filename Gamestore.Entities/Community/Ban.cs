namespace Gamestore.Entities.Community;
public class Ban
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public DateTime BanStart { get; set; } = DateTime.Now;

    public DateTime? BanEnd { get; set; }

    public bool IsPermanent { get; set; }
}
