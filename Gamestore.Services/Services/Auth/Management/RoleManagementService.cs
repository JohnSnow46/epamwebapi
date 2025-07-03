using Gamestore.Data.Interfaces;
using Gamestore.Entities.Auth;
using Gamestore.Services.Dto.AuthDto;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.Auth.Management;

/// <summary>
/// Service for role management operations performed by administrators
/// </summary>
public class RoleManagementService(IUnitOfWork unitOfWork, ILogger<RoleManagementService> logger) : IRoleManagementService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<RoleManagementService> _logger = logger;

    #region Public Methods

    public async Task<IEnumerable<RoleDto>> GetAllRolesForManagementAsync()
    {
        _logger.LogInformation("Getting all roles for management");

        var roles = await _unitOfWork.Roles.GetAllWithPermissionsAsync();
        var roleDtos = roles.Select(r => new RoleDto
        {
            Id = r.Id.ToString(),
            Name = r.Name
        }).ToList();

        _logger.LogInformation("Retrieved {Count} roles for management", roleDtos.Count);
        return roleDtos;
    }

    public async Task<RoleDto> GetRoleForManagementAsync(string id)
    {
        ValidateStringParameter(id, nameof(id));
        _logger.LogInformation("Getting role for management by ID: {RoleId}", id);

        var role = await TryGetRoleByIdOrName(id) ?? throw new KeyNotFoundException($"Role with ID '{id}' not found");
        var roleDto = new RoleDto
        {
            Id = role.Id.ToString(),
            Name = role.Name
        };

        _logger.LogInformation("Found role: {RoleName} with ID: {RoleId}", role.Name, role.Id);
        return roleDto;
    }

    public async Task<object> CreateRoleForManagementAsync(AddRoleRequest request)
    {
        _logger.LogInformation("Creating new role for management");

        var roleName = request.Role.Name;
        var permissions = request.Permissions ?? new List<string>();

        // Check if role already exists
        if (await _unitOfWork.Roles.RoleExistsAsync(roleName))
        {
            throw new InvalidOperationException($"Role with name '{roleName}' already exists");
        }

        // Validate permissions if provided
        if (permissions.Count > 0)
        {
            await ValidatePermissions(permissions);
        }

        // Create role (use level 10 for custom roles, above system roles)
        var newRole = await CreateRole(roleName, $"Custom role: {roleName}", 10, false);

        // Assign permissions to the new role
        if (permissions.Count > 0)
        {
            await AssignPermissionsToRole(newRole.Name, permissions);
        }

        _logger.LogInformation("Role {RoleName} created successfully with ID: {RoleId}",
            roleName, newRole.Id);

        return new
        {
            id = newRole.Id.ToString(),
            name = newRole.Name,
            permissions = permissions,
            createdAt = DateTime.UtcNow
        };
    }

    public async Task<object> UpdateRoleForManagementAsync(UpdateRoleRequest request)
    {
        _logger.LogInformation("Updating role for management");

        var roleId = request.Role.Id;
        var permissions = request.Permissions ?? new List<string>();

        var existingRole = await TryGetRoleByIdOrName(roleId) ?? throw new KeyNotFoundException($"Role with ID '{roleId}' not found");

        // Prevent modification of system roles
        if (existingRole.IsSystemRole)
        {
            throw new InvalidOperationException($"Cannot modify system role '{existingRole.Name}'");
        }

        _logger.LogInformation("Role name updates not implemented. Only updating permissions for role: {RoleName}",
            existingRole.Name);

        // Update permissions if provided
        if (permissions.Count > 0)
        {
            await ValidatePermissions(permissions);
            await UpdateRolePermissions(existingRole.Name, permissions);
        }

        _logger.LogInformation("Role {RoleName} (ID: {RoleId}) permissions updated successfully",
            existingRole.Name, existingRole.Id);

        return new
        {
            id = existingRole.Id.ToString(),
            name = existingRole.Name,
            permissions = permissions,
            updatedAt = DateTime.UtcNow
        };
    }

    public async Task<object> DeleteRoleForManagementAsync(string id)
    {
        ValidateStringParameter(id, nameof(id));
        _logger.LogInformation("Deleting role for management with ID: {RoleId}", id);

        var role = await TryGetRoleByIdOrName(id) ?? throw new KeyNotFoundException($"Role with ID '{id}' not found");

        if (role.IsSystemRole)
        {
            throw new InvalidOperationException($"Cannot delete system role '{role.Name}'");
        }

        // Check if role is assigned to users
        var userRoles = await _unitOfWork.UserRoles.GetByRoleIdAsync(role.Id);
        if (userRoles.Any())
        {
            throw new InvalidOperationException(
                $"Cannot delete role '{role.Name}'. It may be assigned to users or be a system role.");
        }

        await _unitOfWork.Roles.DeleteAsync(role.Id);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Role {RoleName} with ID {RoleId} has been deleted",
            role.Name, id);

        return new
        {
            message = $"Role '{role.Name}' has been deleted successfully",
            timestamp = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<string>> GetAllPermissionsForManagementAsync()
    {
        _logger.LogInformation("Getting all permissions for management");

        var permissions = await _unitOfWork.Permissions.GetAllAsync();
        var permissionNames = permissions.Select(p => p.Name).ToList();

        _logger.LogInformation("Retrieved {Count} permissions for management", permissionNames.Count);
        return permissionNames;
    }

    public async Task<IEnumerable<string>> GetRolePermissionsForManagementAsync(string id)
    {
        ValidateStringParameter(id, nameof(id));
        _logger.LogInformation("Getting permissions for role ID: {RoleId} for management", id);

        var role = await TryGetRoleByIdOrName(id) ?? throw new KeyNotFoundException($"Role with ID '{id}' not found");
        var permissions = await _unitOfWork.Permissions.GetPermissionsByRoleAsync(role.Id);
        var permissionNames = permissions.Select(p => p.Name).ToList();

        _logger.LogInformation("Retrieved {Count} permissions for role: {RoleName}",
            permissionNames.Count, role.Name);
        return permissionNames;
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Try to get role by GUID ID first, then by name for backward compatibility
    /// </summary>
    private async Task<Role?> TryGetRoleByIdOrName(string idOrName)
    {
        // First try to parse as GUID and get by ID
        if (Guid.TryParse(idOrName, out var roleId))
        {
            var allRoles = await _unitOfWork.Roles.GetAllAsync();
            var roleById = allRoles.FirstOrDefault(r => r.Id == roleId);
            if (roleById != null)
            {
                return roleById;
            }
        }

        // If not found by ID or not a valid GUID, try by name
        return await _unitOfWork.Roles.GetByNameAsync(idOrName);
    }

    private async Task<Role> CreateRole(string name, string description, int level, bool isSystemRole)
    {
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

        return role;
    }

    private async Task ValidatePermissions(List<string> permissions)
    {
        var allPermissions = await _unitOfWork.Permissions.GetAllAsync();
        var availablePermissions = allPermissions.Select(p => p.Name).ToList();

        var invalidPermissions = permissions.Where(p => !availablePermissions.Contains(p)).ToList();
        if (invalidPermissions.Count > 0)
        {
            throw new ArgumentException($"Invalid permissions: {string.Join(", ", invalidPermissions)}");
        }
    }

    private async Task AssignPermissionsToRole(string roleName, List<string> permissions)
    {
        foreach (var permissionName in permissions)
        {
            await AssignPermissionToRole(roleName, permissionName);
        }
    }

    private async Task AssignPermissionToRole(string roleName, string permissionName)
    {
        var role = await _unitOfWork.Roles.GetByNameAsync(roleName);
        var permission = await _unitOfWork.Permissions.GetByNameAsync(permissionName);

        if (role == null || permission == null)
        {
            return;
        }

        var rolePermission = new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = role.Id,
            PermissionId = permission.Id,
            GrantedAt = DateTime.UtcNow
        };

        await _unitOfWork.RolePermissions.AddAsync(rolePermission);
    }

    private async Task UpdateRolePermissions(string roleName, List<string> permissions)
    {
        // Remove all current permissions
        var currentPermissions = await _unitOfWork.Permissions.GetPermissionsByRoleAsync(
            (await _unitOfWork.Roles.GetByNameAsync(roleName))!.Id);

        foreach (var permission in currentPermissions)
        {
            await RemovePermissionFromRole(roleName, permission.Name);
        }

        // Add new permissions
        await AssignPermissionsToRole(roleName, permissions);
    }

    private async Task RemovePermissionFromRole(string roleName, string permissionName)
    {
        var role = await _unitOfWork.Roles.GetByNameAsync(roleName);
        var permission = await _unitOfWork.Permissions.GetByNameAsync(permissionName);

        if (role == null || permission == null)
        {
            return;
        }

        var rolePermissions = await _unitOfWork.RolePermissions.GetAllAsync();
        var rolePermission = rolePermissions
            .FirstOrDefault(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);

        if (rolePermission != null)
        {
            await _unitOfWork.RolePermissions.DeleteAsync(rolePermission.Id);
        }
    }

    private static void ValidateStringParameter(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} cannot be null or empty", parameterName);
        }
    }

    #endregion
}