namespace Gamestore.Entities.Community;

/// <summary>
/// Represents a user ban entity in the community moderation system.
/// Manages user access restrictions including temporary and permanent bans,
/// providing comprehensive moderation capabilities for community management and user behavior control.
/// </summary>
public class Ban
{
    /// <summary>
    /// Gets or sets the unique identifier for the ban record.
    /// This serves as the primary key for the ban entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the username of the banned user.
    /// This identifies which user account is subject to the ban restrictions.
    /// Used for ban status checking during authentication and authorization processes.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the ban becomes effective.
    /// This marks the beginning of the ban period and is automatically set to the current time
    /// when a new ban is created. Used to determine if a ban is currently active.
    /// </summary>
    public DateTime BanStart { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets the timestamp when the ban expires and the user regains access.
    /// This is nullable to support permanent bans that have no expiration date.
    /// For temporary bans, this defines when the user can access the system again.
    /// </summary>
    public DateTime? BanEnd { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a permanent ban.
    /// When true, the ban never expires and the BanEnd date is ignored.
    /// Permanent bans typically require administrative intervention to remove.
    /// When false, the ban expires at the time specified in BanEnd.
    /// </summary>
    public bool IsPermanent { get; set; }
}