using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;
public class UserRoleRepository(GameCatalogDbContext context) : Repository<UserRole>(context), IUserRoleRepository
{
    private readonly GameCatalogDbContext _context = context;

    public async Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserRole>> GetByRoleIdAsync(Guid roleId)
    {
        return await _context.UserRoles
            .Include(ur => ur.User)
            .Where(ur => ur.RoleId == roleId)
            .ToListAsync();
    }

    public async Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId)
    {
        return await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
    }

    public async Task AddUserToRoleAsync(Guid userId, Guid roleId, Guid? assignedBy = null)
    {
        var existingUserRole = await GetUserRoleAsync(userId, roleId);
        if (existingUserRole == null)
        {
            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = roleId,
                AssignedBy = assignedBy,
                AssignedAt = DateTime.UtcNow
            };

            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveUserFromRoleAsync(Guid userId, Guid roleId)
    {
        var userRole = await GetUserRoleAsync(userId, roleId);
        if (userRole != null)
        {
            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveAllUserRolesAsync(Guid userId)
    {
        var userRoles = await GetByUserIdAsync(userId);
        _context.UserRoles.RemoveRange(userRoles);
        await _context.SaveChangesAsync();
    }
}