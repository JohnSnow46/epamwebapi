using Gamestore.Entities.Auth;

namespace Gamestore.Data.Interfaces;
public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name);
    Task<Role?> GetByNameWithPermissionsAsync(string name);
    Task<IEnumerable<Role>> GetAllWithPermissionsAsync();
    Task<bool> RoleExistsAsync(string name);
    Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId);
}