using Gamestore.Data.Interfaces;
using Gamestore.Entities.Auth;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Gamestore.Services.Services.Auth;

/// <summary>
/// Core database role service for authentication and authorization operations
/// Management operations have been moved to specialized services
/// </summary>
public class DatabaseRoleService(
    IUnitOfWork unitOfWork,
    IConfiguration configuration,
    ILogger<DatabaseRoleService> logger) : IDatabaseRoleService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<DatabaseRoleService> _logger = logger;

    #region Public User Management Methods

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _unitOfWork.Users.GetByEmailWithRolesAsync(email);
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _unitOfWork.Users.GetByIdWithRolesAsync(id);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _unitOfWork.Users.GetAllWithRolesAsync();
    }

    public async Task<User> CreateUserAsync(string email, string firstName, string lastName, string passwordHash, string? roleName = null)
    {
        if (await UserExistsAsync(email))
        {
            throw new InvalidOperationException($"User with email {email} already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = passwordHash,
            IsActive = true,
            IsEmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        var roleToAssign = roleName ?? GetDefaultRoleName();
        if (!string.IsNullOrEmpty(roleToAssign))
        {
            await AssignUserToRoleAsync(email, roleToAssign);
        }

        _logger.LogInformation("Created user: {Email} with role: {Role}", email, roleToAssign);
        return user;
    }

    public async Task<User> UpdateUserAsync(Guid userId, string? firstName = null, string? lastName = null, string? email = null)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId) ?? throw new KeyNotFoundException($"User with ID {userId} not found");

        if (!string.IsNullOrEmpty(firstName))
        {
            user.FirstName = firstName;
        }

        if (!string.IsNullOrEmpty(lastName))
        {
            user.LastName = lastName;
        }

        if (!string.IsNullOrEmpty(email))
        {
            user.Email = email.ToLowerInvariant();
        }

        user.LastLoginAt = DateTime.UtcNow; // Update last login time

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Updated user: {UserId}", userId);
        return user;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        await _unitOfWork.UserRoles.RemoveAllUserRolesAsync(userId);

        await _unitOfWork.Users.DeleteAsync(userId);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Deleted user: {UserId}", userId);
        return true;
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        return await _unitOfWork.Users.EmailExistsAsync(email);
    }

    public async Task<bool> ValidateUserPasswordAsync(string email, string password)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);
        return user != null && user.IsActive && VerifyPassword(password, user.PasswordHash);
    }

    #endregion

    #region Public Role Management Methods

    public async Task<string> GetUserRoleAsync(string email)
    {
        var user = await _unitOfWork.Users.GetByEmailWithRolesAsync(email);

        if (user?.UserRoles?.Count > 0)
        {
            var highestRole = user.UserRoles
                .OrderBy(ur => ur.Role.Level)
                .First()
                .Role.Name;

            return highestRole;
        }

        return GetGuestRoleName();
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(string email)
    {
        var user = await _unitOfWork.Users.GetByEmailWithRolesAsync(email);

        return user?.UserRoles?.Count == 0 || user?.UserRoles == null
            ? [GetGuestRoleName()]
            : user.UserRoles.Select(ur => ur.Role.Name);
    }

    public async Task<bool> AssignUserToRoleAsync(string email, string roleName, Guid? assignedBy = null)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);
        var role = await _unitOfWork.Roles.GetByNameAsync(roleName);

        if (user == null || role == null)
        {
            _logger.LogWarning("Failed to assign role - User or role not found. Email: {Email}, Role: {Role}", email, roleName);
            return false;
        }

        await _unitOfWork.UserRoles.AddUserToRoleAsync(user.Id, role.Id, assignedBy);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Assigned role {Role} to user {Email}", roleName, email);
        return true;
    }

    public async Task<bool> RemoveUserFromRoleAsync(string email, string roleName)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);
        var role = await _unitOfWork.Roles.GetByNameAsync(roleName);

        if (user == null || role == null)
        {
            return false;
        }

        await _unitOfWork.UserRoles.RemoveUserFromRoleAsync(user.Id, role.Id);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Removed role {Role} from user {Email}", roleName, email);
        return true;
    }

    public async Task<bool> UserHasRoleAsync(string email, string roleName)
    {
        var userRoles = await GetUserRolesAsync(email);
        return userRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<bool> UserHasPermissionAsync(string email, string permissionName)
    {
        var user = await _unitOfWork.Users.GetByEmailWithRolesAsync(email);

        if (user?.UserRoles?.Count == 0 || user?.UserRoles == null)
        {
            return false;
        }

        foreach (var userRole in user.UserRoles)
        {
            var role = await _unitOfWork.Roles.GetByNameWithPermissionsAsync(userRole.Role.Name);
            if (role?.RolePermissions?.Any(rp =>
                string.Equals(rp.Permission.Name, permissionName, StringComparison.OrdinalIgnoreCase)) == true)
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Public Role Definitions Methods

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await _unitOfWork.Roles.GetAllWithPermissionsAsync();
    }

    public async Task<Role?> GetRoleByNameAsync(string name)
    {
        return await _unitOfWork.Roles.GetByNameWithPermissionsAsync(name);
    }

    public async Task<Role> CreateRoleAsync(string name, string description, int level, bool isSystemRole = false)
    {
        if (await RoleExistsAsync(name))
        {
            throw new InvalidOperationException($"Role {name} already exists");
        }

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Level = level,
            IsSystemRole = isSystemRole,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Roles.AddAsync(role);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Created role: {RoleName}", name);
        return role;
    }

    public async Task<bool> DeleteRoleAsync(string name)
    {
        var role = await _unitOfWork.Roles.GetByNameAsync(name);
        if (role == null)
        {
            return false;
        }

        if (role.IsSystemRole)
        {
            _logger.LogWarning("Attempted to delete system role: {RoleName}", name);
            return false;
        }

        var userRoles = await _unitOfWork.UserRoles.GetByRoleIdAsync(role.Id);
        if (userRoles.Any())
        {
            _logger.LogWarning("Cannot delete role {RoleName} - users are assigned to it", name);
            return false;
        }

        await _unitOfWork.Roles.DeleteAsync(role.Id);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Deleted role: {RoleName}", name);
        return true;
    }

    public async Task<bool> RoleExistsAsync(string name)
    {
        return await _unitOfWork.Roles.RoleExistsAsync(name);
    }

    #endregion

    #region Public Permission Management Methods

    public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
    {
        return await _unitOfWork.Permissions.GetAllAsync();
    }

    public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(string roleName)
    {
        var role = await _unitOfWork.Roles.GetByNameAsync(roleName);
        return role == null
            ? Enumerable.Empty<Permission>()
            : await _unitOfWork.Permissions.GetPermissionsByRoleAsync(role.Id);
    }

    public async Task<bool> AssignPermissionToRoleAsync(string roleName, string permissionName)
    {
        var role = await _unitOfWork.Roles.GetByNameAsync(roleName);
        var permission = await _unitOfWork.Permissions.GetByNameAsync(permissionName);

        if (role == null || permission == null)
        {
            return false;
        }

        // Check if permission is already assigned to role
        var existingRolePermissions = await _unitOfWork.RolePermissions.GetAllAsync();
        var existingAssignment = existingRolePermissions
            .FirstOrDefault(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);

        if (existingAssignment != null)
        {
            _logger.LogInformation("Permission {Permission} already assigned to role {Role}", permissionName, roleName);
            return true;
        }

        var rolePermission = new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = role.Id,
            PermissionId = permission.Id,
            GrantedAt = DateTime.UtcNow
        };

        await _unitOfWork.RolePermissions.AddAsync(rolePermission);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Assigned permission {Permission} to role {Role}", permissionName, roleName);
        return true;
    }

    public async Task<bool> RemovePermissionFromRoleAsync(string roleName, string permissionName)
    {
        var role = await _unitOfWork.Roles.GetByNameAsync(roleName);
        var permission = await _unitOfWork.Permissions.GetByNameAsync(permissionName);

        if (role == null || permission == null)
        {
            return false;
        }

        var rolePermissions = await _unitOfWork.RolePermissions.GetAllAsync();
        var rolePermission = rolePermissions
            .FirstOrDefault(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);

        if (rolePermission != null)
        {
            await _unitOfWork.RolePermissions.DeleteAsync(rolePermission.Id);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Removed permission {Permission} from role {Role}", permissionName, roleName);
            return true;
        }

        return false;
    }

    #endregion

    #region Public Migration and Seeding Methods

    public async Task SeedDefaultRolesAndPermissionsAsync()
    {
        _logger.LogInformation("Starting to seed default roles and permissions");

        // Create default permissions
        await SeedPermissions();

        // Create default roles
        await SeedRoles();

        // Assign permissions to roles
        await AssignPermissionsToRoles();

        _logger.LogInformation("Completed seeding default roles and permissions");
    }

    public async Task MigrateHardcodedUsersAsync()
    {
        _logger.LogInformation("Starting migration of hardcoded users");

        var hardcodedUsers = GetHardcodedUsers();

        foreach (var (email, userInfo) in hardcodedUsers)
        {
            if (!await UserExistsAsync(email))
            {
                var passwordHash = HashPassword(userInfo.password);
                var role = DetermineUserRole(email);

                await CreateUserAsync(email, userInfo.firstName, userInfo.lastName, passwordHash, role);
                _logger.LogInformation("Migrated user: {Email} with role: {Role}", email, role);
            }
            else
            {
                _logger.LogInformation("User already exists, skipping: {Email}", email);
            }
        }

        _logger.LogInformation("Completed migration of hardcoded users");
    }

    #endregion

    #region Private Helper Methods

    private async Task SeedPermissions()
    {
        var permissions = new[]
        {
            // Game permissions
            new { Name = "ViewGame", Description = "View games", Category = "Games" },
            new { Name = "AddGame", Description = "Add new games", Category = "Games" },
            new { Name = "UpdateGame", Description = "Update existing games", Category = "Games" },
            new { Name = "DeleteGame", Description = "Delete games", Category = "Games" },
            new { Name = "ViewDeletedGames", Description = "View deleted games", Category = "Games" },
            new { Name = "EditDeletedGames", Description = "Edit deleted games", Category = "Games" },

            // Genre permissions
            new { Name = "ViewGenre", Description = "View genres", Category = "Business" },
            new { Name = "AddGenre", Description = "Add new genres", Category = "Business" },
            new { Name = "UpdateGenre", Description = "Update existing genres", Category = "Business" },
            new { Name = "DeleteGenre", Description = "Delete genres", Category = "Business" },

            // Platform permissions
            new { Name = "ViewPlatform", Description = "View platforms", Category = "Business" },
            new { Name = "AddPlatform", Description = "Add new platforms", Category = "Business" },
            new { Name = "UpdatePlatform", Description = "Update existing platforms", Category = "Business" },
            new { Name = "DeletePlatform", Description = "Delete platforms", Category = "Business" },

            // Publisher permissions
            new { Name = "ViewPublisher", Description = "View publishers", Category = "Business" },
            new { Name = "AddPublisher", Description = "Add new publishers", Category = "Business" },
            new { Name = "UpdatePublisher", Description = "Update existing publishers", Category = "Business" },
            new { Name = "DeletePublisher", Description = "Delete publishers", Category = "Business" },

            // Comment permissions
            new { Name = "ViewComments", Description = "View comments", Category = "Comments" },
            new { Name = "AddComment", Description = "Add comments", Category = "Comments" },
            new { Name = "DeleteComment", Description = "Delete comments", Category = "Comments" },
            new { Name = "ModerateComments", Description = "Moderate comments", Category = "Comments" },
            new { Name = "BanUsers", Description = "Ban users from commenting", Category = "Comments" },

            // Order permissions
            new { Name = "ViewOrders", Description = "View orders", Category = "Orders" },
            new { Name = "EditOrders", Description = "Edit orders", Category = "Orders" },
            new { Name = "ViewOrderHistory", Description = "View order history", Category = "Orders" },
            new { Name = "ShipOrders", Description = "Ship orders", Category = "Orders" },

            // User management permissions
            new { Name = "ViewUsers", Description = "View users", Category = "Users" },
            new { Name = "AddUser", Description = "Add new users", Category = "Users" },
            new { Name = "UpdateUser", Description = "Update existing users", Category = "Users" },
            new { Name = "DeleteUser", Description = "Delete users", Category = "Users" },

            // Role management permissions
            new { Name = "ViewRoles", Description = "View roles", Category = "Roles" },
            new { Name = "AddRole", Description = "Add new roles", Category = "Roles" },
            new { Name = "UpdateRole", Description = "Update existing roles", Category = "Roles" },
            new { Name = "DeleteRole", Description = "Delete roles", Category = "Roles" },
        };

        foreach (var perm in permissions)
        {
            if (!await _unitOfWork.Permissions.PermissionExistsAsync(perm.Name))
            {
                var permission = new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = perm.Name,
                    Description = perm.Description,
                    Category = perm.Category
                };

                await _unitOfWork.Permissions.AddAsync(permission);
            }
        }

        await _unitOfWork.CompleteAsync();
    }

    private async Task SeedRoles()
    {
        var roles = new[]
        {
            new { Name = Roles.Administrator, Description = "Full system access", Level = 0, IsSystem = true },
            new { Name = Roles.Manager, Description = "Manage business entities and orders", Level = 1, IsSystem = true },
            new { Name = Roles.Moderator, Description = "Moderate comments and ban users", Level = 2, IsSystem = true },
            new { Name = Roles.User, Description = "Regular user access", Level = 3, IsSystem = true },
            new { Name = Roles.Guest, Description = "Read-only access", Level = 4, IsSystem = true }
        };

        foreach (var role in roles)
        {
            if (!await RoleExistsAsync(role.Name))
            {
                await CreateRoleAsync(role.Name, role.Description, role.Level, role.IsSystem);
                _logger.LogInformation("Created role: {RoleName}", role.Name);
            }
            else
            {
                _logger.LogInformation("Role already exists: {RoleName}", role.Name);
            }
        }
    }

    private async Task AssignPermissionsToRoles()
    {
        // Administrator gets all permissions
        var adminRole = await _unitOfWork.Roles.GetByNameAsync(Roles.Administrator);
        if (adminRole != null)
        {
            var allPermissions = await _unitOfWork.Permissions.GetAllAsync();
            foreach (var permission in allPermissions)
            {
                await AssignPermissionToRoleAsync(Roles.Administrator, permission.Name);
            }
        }

        // Manager permissions
        var managerPermissions = new[]
        {
            "ViewGame", "AddGame", "UpdateGame", "DeleteGame",
            "ViewGenre", "AddGenre", "UpdateGenre", "DeleteGenre",
            "ViewPlatform", "AddPlatform", "UpdatePlatform", "DeletePlatform",
            "ViewPublisher", "AddPublisher", "UpdatePublisher", "DeletePublisher",
            "ViewComments", "AddComment", "DeleteComment", "ModerateComments", "BanUsers",
            "ViewOrders", "EditOrders", "ViewOrderHistory", "ShipOrders"
        };

        foreach (var permission in managerPermissions)
        {
            await AssignPermissionToRoleAsync(Roles.Manager, permission);
        }

        // Moderator permissions
        var moderatorPermissions = new[]
        {
            "ViewGame", "ViewGenre", "ViewPlatform", "ViewPublisher",
            "ViewComments", "AddComment", "DeleteComment", "ModerateComments", "BanUsers"
        };

        foreach (var permission in moderatorPermissions)
        {
            await AssignPermissionToRoleAsync(Roles.Moderator, permission);
        }

        // User permissions
        var userPermissions = new[]
        {
            "ViewGame", "ViewGenre", "ViewPlatform", "ViewPublisher",
            "ViewComments", "AddComment"
        };

        foreach (var permission in userPermissions)
        {
            await AssignPermissionToRoleAsync(Roles.User, permission);
        }

        // Guest permissions
        var guestPermissions = new[]
        {
            "ViewGame", "ViewGenre", "ViewPlatform", "ViewPublisher", "ViewComments"
        };

        foreach (var permission in guestPermissions)
        {
            await AssignPermissionToRoleAsync(Roles.Guest, permission);
        }
    }

    private Dictionary<string, (string password, string firstName, string lastName)> GetHardcodedUsers()
    {
        var users = new Dictionary<string, (string password, string firstName, string lastName)>();

        var usersSection = _configuration.GetSection("HardcodedUsers");
        foreach (var userSection in usersSection.GetChildren())
        {
            var email = userSection.Key.ToLowerInvariant();
            var password = userSection["password"] ?? string.Empty;
            var firstName = userSection["firstName"] ?? "User";
            var lastName = userSection["lastName"] ?? "Name";

            if (!string.IsNullOrEmpty(password))
            {
                users[email] = (password, firstName, lastName);
            }
        }

        return users;
    }

    private string DetermineUserRole(string email)
    {
        // Check specific users first
        var specificUsersSection = _configuration.GetSection("RoleAssignment:SpecificUsers");
        var specificUsers = specificUsersSection.GetChildren()
            .ToDictionary(x => x.Key, x => x.Value ?? string.Empty);

        if (specificUsers.TryGetValue(email, out var role) && !string.IsNullOrEmpty(role))
        {
            return role;
        }

        // Pattern matching for backward compatibility
        return email switch
        {
            var e when e.Contains("admin") => Roles.Administrator,
            var e when e.Contains("manager") => Roles.Manager,
            var e when e.Contains("moderator") => Roles.Moderator,
            var e when e.Contains("user") => Roles.User,
            _ => GetDefaultRoleName()
        };
    }

    private string GetDefaultRoleName()
    {
        var defaultRole = _configuration["RoleAssignment:DefaultRole"];
        return !string.IsNullOrEmpty(defaultRole) && Roles.AllRoles.Contains(defaultRole)
            ? defaultRole
            : Roles.User;
    }

    private string GetGuestRoleName()
    {
        var guestRole = _configuration["RoleAssignment:GuestRole"];
        return !string.IsNullOrEmpty(guestRole) && Roles.AllRoles.Contains(guestRole)
            ? guestRole
            : Roles.Guest;
    }

    private static string HashPassword(string password)
    {
        // Simple password hashing - in production use BCrypt or similar
        var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password + "GamestoreSalt2024"));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        var computedHash = HashPassword(password);
        return computedHash == hash;
    }

    #endregion
}