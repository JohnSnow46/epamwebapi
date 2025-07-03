using Gamestore.Entities.Auth;

namespace Gamestore.Data.Interfaces;
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByEmailWithRolesAsync(string email);
    Task<User?> GetByIdWithRolesAsync(Guid id);
    Task<IEnumerable<User>> GetAllWithRolesAsync();
    Task<bool> EmailExistsAsync(string email);
    Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);
    Task UpdateLastLoginAsync(Guid userId);
}
