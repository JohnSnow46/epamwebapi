using Gamestore.Entities.Auth;

namespace Gamestore.Data.Interfaces;
public interface IUserRoleRepository : IRepository<UserRole>
{
    Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<UserRole>> GetByRoleIdAsync(Guid roleId);
    Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId);
    Task AddUserToRoleAsync(Guid userId, Guid roleId, Guid? assignedBy = null);
    Task RemoveUserFromRoleAsync(Guid userId, Guid roleId);
    Task RemoveAllUserRolesAsync(Guid userId);
}
