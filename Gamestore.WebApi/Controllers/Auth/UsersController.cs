using Gamestore.Entities.ErrorModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Gamestore.Services.Dto.AuthDto;
using Gamestore.WebApi.Extensions;
using Gamestore.Services.Interfaces;

namespace Gamestore.WebApi.Controllers.Auth;

/// <summary>
/// Controller for user management operations.
/// Uses IUserManagementService for all management operations.
/// Only administrators can access these endpoints.
/// </summary>
[ApiController]
[Route("api")]
public class UsersController(IUserManagementService userManagementService, ILogger<UsersController> logger) : ControllerBase
{
    private readonly IUserManagementService _userManagementService = userManagementService;
    private readonly ILogger<UsersController> _logger = logger;

    /// <summary>
    /// US3 - Get all users endpoint
    /// Epic 9: Only Admin can manage users
    /// </summary>
    [HttpGet("users")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            _logger.LogInformation("Getting all users by admin: {Admin}", User.GetUserEmail());

            var users = await _userManagementService.GetAllUsersForManagementAsync();

            _logger.LogInformation("Retrieved {Count} users", users.Count());
            return Ok(users);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving all users");
        }
    }

    /// <summary>
    /// US4 - Get user by id endpoint
    /// Epic 9: Only Admin can view specific user details
    /// </summary>
    [HttpGet("users/{id}")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> GetUserById(string id)
    {
        try
        {
            _logger.LogInformation("Getting user by ID: {UserId} by admin: {Admin}", id, User.GetUserEmail());

            var user = await _userManagementService.GetUserForManagementAsync(id);

            _logger.LogInformation("Found user with ID: {UserId}", id);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error retrieving user with ID: {id}");
        }
    }

    /// <summary>
    /// US5 - Delete user by id endpoint
    /// Epic 9: Only Admin can delete users
    /// Includes protection against self-deletion
    /// </summary>
    [HttpDelete("users/{id}")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> DeleteUserById(string id)
    {
        try
        {
            _logger.LogInformation("Deleting user with ID: {UserId} by admin: {Admin}", id, User.GetUserEmail());

            var currentUserEmail = User.GetUserEmail();
            if (string.IsNullOrEmpty(currentUserEmail))
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Unable to identify current user",
                    StatusCode = StatusCodes.Status400BadRequest,
                    ErrorId = Guid.NewGuid().ToString()
                });
            }

            var result = await _userManagementService.DeleteUserForManagementAsync(id, currentUserEmail);

            _logger.LogInformation("Successfully deleted user with ID: {UserId}", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error deleting user with ID: {id}");
        }
    }

    /// <summary>
    /// US9 - Add user endpoint
    /// Epic 9: Only Admin can create users
    /// </summary>
    [HttpPost("users")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> AddUser([FromBody] CreateUserRequest request)
    {
        try
        {
            _logger.LogInformation("Creating user: {UserName} by admin: {Admin}",
                request.User.Name, User.GetUserEmail());

            var result = await _userManagementService.CreateUserForManagementAsync(request);

            // Extract ID from result for CreatedAtAction
            var resultDict = result.GetType().GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(result));
            var userId = resultDict.TryGetValue("id", out var value) ? value?.ToString() : "unknown";

            _logger.LogInformation("Successfully created user with ID: {UserId}", userId);
            return CreatedAtAction(nameof(GetUserById), new { id = userId }, result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error creating user");
        }
    }

    /// <summary>
    /// US10 - Update user endpoint
    /// Epic 9: Only Admin can update users
    /// </summary>
    [HttpPut("users/{id}")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            _logger.LogInformation("Updating user: {UserId} by admin: {Admin}", id, User.GetUserEmail());

            var result = await _userManagementService.UpdateUserForManagementAsync(id, request);

            _logger.LogInformation("Successfully updated user with ID: {UserId}", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error updating user");
        }
    }

    /// <summary>
    /// US11 - Get user roles endpoint
    /// Epic 9: Only Admin can view user roles
    /// </summary>
    [HttpGet("users/{id}/roles")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> GetUserRoles(string id)
    {
        try
        {
            _logger.LogInformation("Getting roles for user ID: {UserId} by admin: {Admin}", id, User.GetUserEmail());

            var roles = await _userManagementService.GetUserRolesForManagementAsync(id);

            _logger.LogInformation("Retrieved {Count} roles for user: {UserId}", roles.Count(), id);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error retrieving roles for user ID: {id}");
        }
    }

    /// <summary>
    /// Additional endpoint: Get current user's info (for profile display)
    /// Epic 9: Any authenticated user can view their own info
    /// </summary>
    [HttpGet("users/me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        try
        {
            var currentUserEmail = User.GetUserEmail();
            if (string.IsNullOrEmpty(currentUserEmail))
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Unable to identify current user",
                    StatusCode = StatusCodes.Status400BadRequest,
                    ErrorId = Guid.NewGuid().ToString()
                });
            }

            _logger.LogInformation("Getting current user info for: {Email}", currentUserEmail);

            // For security, we only return basic info for non-admin users
            var userInfo = new
            {
                email = currentUserEmail,
                name = User.GetUserName(),
                role = User.GetUserRole(),
                isAuthenticated = true
            };

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving current user information");
        }
    }

    /// <summary>
    /// Additional endpoint: Check if user can perform specific action
    /// Epic 9: Used by frontend to show/hide UI elements
    /// </summary>
    [HttpGet("users/permissions/{permission}")]
    [Authorize]
    public IActionResult CheckPermission(string permission)
    {
        try
        {
            _logger.LogInformation("Checking permission {Permission} for user: {Email}",
                permission, User.GetUserEmail());

            var hasPermission = permission.ToLowerInvariant() switch
            {
                "manage-users" => User.CanManageUsers(),
                "manage-roles" => User.CanManageRoles(),
                "manage-games" => User.CanManageGames(),
                "manage-orders" => User.CanManageOrders(),
                "moderate-comments" => User.CanModerateComments(),
                "view-deleted-games" => User.CanViewDeletedGames(),
                _ => false
            };

            return Ok(new
            {
                permission,
                hasPermission,
                userRole = User.GetUserRole(),
                checkedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error checking permission: {permission}");
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Handles exceptions and maps them to appropriate HTTP responses
    /// </summary>
    private ObjectResult HandleException(Exception ex, string logMessage)
    {
        _logger.LogError(ex, "{LogMessage}: {ErrorMessage}", logMessage, ex.Message);

        return ex switch
        {
            // Validation errors -> Bad Request (400)
            ArgumentException => BadRequest(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status400BadRequest,
                ErrorId = Guid.NewGuid().ToString()
            }),

            // Not found errors -> Not Found (404)
            KeyNotFoundException => NotFound(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status404NotFound,
                ErrorId = Guid.NewGuid().ToString()
            }),

            // Conflict errors (already exists, self-deletion) -> Conflict (409)
            InvalidOperationException when ex.Message.Contains("already exists") => Conflict(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status409Conflict,
                ErrorId = Guid.NewGuid().ToString()
            }),

            // Other business logic errors (self-deletion, etc.) -> Bad Request (400)
            InvalidOperationException => BadRequest(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status400BadRequest,
                ErrorId = Guid.NewGuid().ToString()
            }),

            // Unexpected errors -> Internal Server Error (500)
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseModel
            {
                Message = "An unexpected error occurred while processing the request.",
                Details = ex.Message,
                StatusCode = StatusCodes.Status500InternalServerError,
                ErrorId = Guid.NewGuid().ToString()
            })
        };
    }

    #endregion
}