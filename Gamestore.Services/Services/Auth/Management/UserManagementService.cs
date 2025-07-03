using System.Security.Cryptography;
using System.Text;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Auth;
using Gamestore.Services.Dto.AuthDto;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.Auth.Management;

/// <summary>
/// Service for user management operations performed by administrators
/// </summary>
public class UserManagementService(IUnitOfWork unitOfWork, ILogger<UserManagementService> logger) : IUserManagementService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<UserManagementService> _logger = logger;

    #region Public Methods

    public async Task<IEnumerable<UserDto>> GetAllUsersForManagementAsync()
    {
        _logger.LogInformation("Getting all users for management");

        var users = await _unitOfWork.Users.GetAllWithRolesAsync();
        var userDtos = users.Select(u => new UserDto
        {
            Id = u.Id.ToString(),
            Name = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            LastLoginAt = u.LastLoginAt,
            Roles = u.GetRoleNames(),
        }).ToList();

        _logger.LogInformation("Retrieved {Count} users for management", userDtos.Count);
        return userDtos;
    }

    public async Task<UserDto> GetUserForManagementAsync(string id)
    {
        ValidateGuidParameter(id, nameof(id));
        _logger.LogInformation("Getting user for management by ID: {UserId}", id);

        var userId = Guid.Parse(id);
        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(userId) ?? throw new KeyNotFoundException($"User with ID '{id}' not found");

        var userDto = new UserDto
        {
            Id = user.Id.ToString(),
            Name = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = user.GetRoleNames(),
        };

        _logger.LogInformation("Found user: {UserEmail} with ID: {UserId}", user.Email, id);
        return userDto;
    }

    public async Task<object> CreateUserForManagementAsync(CreateUserRequest request)
    {
        _logger.LogInformation("Creating user for management");

        var userName = request.User.Name;
        var password = request.Password;
        var roleIds = request.Roles ?? new List<string>();

        // Convert to internal format
        var email = userName.Contains('@') ? userName : $"{userName}@gamestore.com";
        var firstName = ParseFirstName(userName);
        var lastName = ParseLastName(userName);

        // Check if user already exists
        if (await _unitOfWork.Users.EmailExistsAsync(email))
        {
            throw new InvalidOperationException($"User with email {email} already exists");
        }

        // Determine role from request
        var roleName = await DetermineRoleFromRequest(roleIds);

        // Create user with proper role
        var passwordHash = HashPassword(password);
        var newUser = await CreateUser(email, firstName, lastName, passwordHash, roleName);

        _logger.LogInformation("User created successfully: {Email} with role: {Role}", email, roleName);

        return new
        {
            id = newUser.Id.ToString(),
            name = newUser.Email,
            role = roleName,
            success = true,
            createdAt = DateTime.UtcNow
        };
    }

    public async Task<object> UpdateUserForManagementAsync(string id, UpdateUserRequest request)
    {
        ValidateGuidParameter(id, nameof(id));
        _logger.LogInformation("Updating user for management: {UserId}", id);

        var userId = Guid.Parse(id);
        var firstName = request.User?.FirstName;
        var lastName = request.User?.LastName;
        var email = request.User?.Email;
        var roleName = request.RoleName;
        var password = request.Password;

        _ = await _unitOfWork.Users.GetByIdAsync(userId) ?? throw new KeyNotFoundException($"User with ID '{id}' not found");

        // Update user details
        var updatedUser = await UpdateUser(userId, firstName, lastName, email);

        // Update role if provided
        if (!string.IsNullOrEmpty(roleName))
        {
            await UpdateUserRole(updatedUser.Email, roleName);
        }

        // Update password if provided
        if (!string.IsNullOrEmpty(password))
        {
            await UpdateUserPassword(userId, password);
        }

        _logger.LogInformation("User {UserEmail} (ID: {UserId}) updated successfully",
            updatedUser.Email, updatedUser.Id);

        var finalRoles = await GetUserRoleNames(updatedUser.Email);

        return new
        {
            id = updatedUser.Id.ToString(),
            email = updatedUser.Email,
            firstName = updatedUser.FirstName,
            lastName = updatedUser.LastName,
            roles = finalRoles.ToArray(),
            updatedBy = "System", // In real app, get from context
            updatedAt = DateTime.UtcNow
        };
    }

    public async Task<object> DeleteUserForManagementAsync(string id, string currentUserEmail)
    {
        ValidateGuidParameter(id, nameof(id));
        ValidateStringParameter(currentUserEmail, nameof(currentUserEmail));
        _logger.LogInformation("Deleting user for management with ID: {UserId}", id);

        var userId = Guid.Parse(id);
        var user = await _unitOfWork.Users.GetByIdAsync(userId) ?? throw new KeyNotFoundException($"User with ID '{id}' not found");

        // Prevent admin from deleting themselves
        if (string.Equals(user.Email, currentUserEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("You cannot delete your own account");
        }

        // Remove user roles first
        await _unitOfWork.UserRoles.RemoveAllUserRolesAsync(userId);

        // Delete user
        await _unitOfWork.Users.DeleteAsync(userId);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("User {UserEmail} with ID {UserId} has been deleted",
            user.Email, id);

        return new
        {
            message = $"User '{user.Email}' has been deleted successfully",
            timestamp = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<RoleDto>> GetUserRolesForManagementAsync(string id)
    {
        ValidateGuidParameter(id, nameof(id));
        _logger.LogInformation("Getting roles for user ID: {UserId} for management", id);

        var userId = Guid.Parse(id);
        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(userId) ?? throw new KeyNotFoundException($"User with ID '{id}' not found");

        // Get actual role DTOs with proper IDs
        var roleDtos = new List<RoleDto>();
        if (user.UserRoles != null && user.UserRoles.Count != 0)
        {
            roleDtos = user.UserRoles.Select(ur => new RoleDto
            {
                Id = ur.Role.Id.ToString(),
                Name = ur.Role.Name
            }).ToList();
        }
        else
        {
            // User has no roles, return default guest role
            var guestRole = await _unitOfWork.Roles.GetByNameAsync("Guest");
            if (guestRole != null)
            {
                roleDtos.Add(new RoleDto
                {
                    Id = guestRole.Id.ToString(),
                    Name = guestRole.Name
                });
            }
        }

        _logger.LogInformation("Retrieved {Count} roles for user: {UserEmail}",
            roleDtos.Count, user.Email);
        return roleDtos;
    }

    #endregion

    #region Private Helper Methods

    private async Task<User> CreateUser(string email, string firstName, string lastName, string passwordHash, string? roleName = null)
    {
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

        // Assign role if provided
        if (!string.IsNullOrEmpty(roleName))
        {
            await AssignUserToRole(email, roleName);
        }

        return user;
    }

    private async Task<User> UpdateUser(Guid userId, string? firstName, string? lastName, string? email)
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

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.CompleteAsync();

        return user;
    }

    private async Task UpdateUserPassword(Guid userId, string newPassword)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId) ?? throw new KeyNotFoundException($"User with ID {userId} not found");
        user.PasswordHash = HashPassword(newPassword);
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Password updated for user ID: {UserId}", userId);
    }

    private async Task UpdateUserRole(string email, string newRoleName)
    {
        // Validate role exists
        if (!await _unitOfWork.Roles.RoleExistsAsync(newRoleName))
        {
            throw new ArgumentException($"Invalid role: {newRoleName}");
        }

        var user = await _unitOfWork.Users.GetByEmailWithRolesAsync(email) ?? throw new KeyNotFoundException($"User with email {email} not found");

        // Remove current roles
        if (user.UserRoles != null && user.UserRoles.Count != 0)
        {
            foreach (var userRole in user.UserRoles.ToList())
            {
                await RemoveUserFromRole(email, userRole.Role.Name);
            }
        }

        // Assign new role
        await AssignUserToRole(email, newRoleName);
    }

    private async Task<IEnumerable<string>> GetUserRoleNames(string email)
    {
        var user = await _unitOfWork.Users.GetByEmailWithRolesAsync(email);
        if (user?.UserRoles != null && user.UserRoles.Count != 0)
        {
            return user.UserRoles.Select(ur => ur.Role.Name);
        }
        return ["Guest"]; // Default role if no roles assigned
    }

    private async Task AssignUserToRole(string email, string roleName)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);
        var role = await _unitOfWork.Roles.GetByNameAsync(roleName);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with email {email} not found");
        }

        if (role == null)
        {
            throw new KeyNotFoundException($"Role {roleName} not found");
        }

        // Check if user already has this role
        var existingUserRole = await _unitOfWork.UserRoles.GetUserRoleAsync(user.Id, role.Id);
        if (existingUserRole != null)
        {
            _logger.LogInformation("User {Email} already has role {Role}", email, roleName);
            return;
        }

        await _unitOfWork.UserRoles.AddUserToRoleAsync(user.Id, role.Id);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Successfully assigned role {Role} to user {Email}", roleName, email);
    }

    private async Task RemoveUserFromRole(string email, string roleName)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);
        var role = await _unitOfWork.Roles.GetByNameAsync(roleName);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with email {email} not found");
        }

        if (role == null)
        {
            throw new KeyNotFoundException($"Role {roleName} not found");
        }

        await _unitOfWork.UserRoles.RemoveUserFromRoleAsync(user.Id, role.Id);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Successfully removed role {Role} from user {Email}", roleName, email);
    }

    private async Task<string> DetermineRoleFromRequest(List<string> roleIds)
    {
        if (roleIds == null || roleIds.Count == 0)
        {
            return "User"; // Default role
        }

        var firstRoleId = roleIds[0];

        // Try to parse as GUID first
        if (Guid.TryParse(firstRoleId, out var roleId))
        {
            var allRoles = await _unitOfWork.Roles.GetAllAsync();
            var role = allRoles.FirstOrDefault(r => r.Id == roleId);
            return role?.Name ?? "User";
        }

        // Check if it's a valid role name
        if (await _unitOfWork.Roles.RoleExistsAsync(firstRoleId))
        {
            return firstRoleId;
        }

        return "User"; // Default fallback
    }

    private static string ParseFirstName(string nameOrEmail)
    {
        if (nameOrEmail.Contains('@'))
        {
            return nameOrEmail.Split('@')[0];
        }
        var parts = nameOrEmail.Split(' ');
        return parts[0];
    }

    private static string ParseLastName(string nameOrEmail)
    {
        if (nameOrEmail.Contains('@'))
        {
            return "User";
        }
        var parts = nameOrEmail.Split(' ');
        return parts.Length > 1 ? parts[^1] : "User";
    }

    private static string HashPassword(string password)
    {
        // Simple password hashing - in production use BCrypt or similar
        var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password + "GamestoreSalt2024"));
        return Convert.ToBase64String(hashedBytes);
    }

    private static void ValidateStringParameter(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} cannot be null or empty", parameterName);
        }
    }

    private static void ValidateGuidParameter(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} cannot be null or empty", parameterName);
        }

        if (!Guid.TryParse(value, out _))
        {
            throw new ArgumentException($"Valid {parameterName} is required", parameterName);
        }
    }

    #endregion
}