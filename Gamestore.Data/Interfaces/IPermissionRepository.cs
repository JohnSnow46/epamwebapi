using Gamestore.Entities.Auth;

namespace Gamestore.Data.Interfaces;
public interface IPermissionRepository : IRepository<Permission>
{
    Task<Permission?> GetByNameAsync(string name);
    Task<IEnumerable<Permission>> GetByCategoryAsync(string category);
    Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(Guid roleId);
    Task<bool> PermissionExistsAsync(string name);
}