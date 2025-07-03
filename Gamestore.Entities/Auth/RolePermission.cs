namespace Gamestore.Entities.Auth;
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public class RolePermission
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    public Guid Id { get; set; }

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
}