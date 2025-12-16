using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Dto.AuthDto;
using Gamestore.Services.Dto.NotificationsDto;
using Gamestore.Services.Interfaces;
using Gamestore.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.Auth;

/// <summary>
/// Controller for user management operations.
/// Uses IUserManagementService for all management operations.
/// Only administrators can access these endpoints.
/// </summary>
[ApiController]
[Route("api")]
public class UsersController(
    IUserManagementService userManagementService,
    INotificationService notificationService,
    IEmailService emailService,
    ILogger<UsersController> logger) : ControllerBase
{
    private readonly IUserManagementService _userManagementService = userManagementService;
    private readonly INotificationService _notificationService = notificationService;
    private readonly ILogger<UsersController> _logger = logger;
    private readonly IEmailService _emailService = emailService;

    /// <summary>
    /// US3 - Get all users endpoint
    /// Epic 9: Only Admin can manage users.
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
    /// Epic 9: Only Admin can view specific user details.
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
    /// Includes protection against self-deletion.
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
                    ErrorId = Guid.NewGuid().ToString(),
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
    /// Epic 9: Only Admin can create users.
    /// </summary>
    [HttpPost("users")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> AddUser([FromBody] CreateUserRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Creating user: {UserName} by admin: {Admin}",
                request.User.Name,
                User.GetUserEmail());

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
    /// Epic 9: Only Admin can update users.
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
    /// Epic 9: Only Admin can view user roles.
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
    /// Epic 9: Any authenticated user can view their own info.
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
                    ErrorId = Guid.NewGuid().ToString(),
                });
            }

            _logger.LogInformation("Getting current user info for: {Email}", currentUserEmail);

            // For security, we only return basic info for non-admin users
            var userInfo = new
            {
                email = currentUserEmail,
                name = User.GetUserName(),
                role = User.GetUserRole(),
                isAuthenticated = true,
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
    /// Epic 9: Used by frontend to show/hide UI elements.
    /// </summary>
    [HttpGet("users/permissions/{permission}")]
    [Authorize]
    public IActionResult CheckPermission(string permission)
    {
        try
        {
            _logger.LogInformation(
                "Checking permission {Permission} for user: {Email}",
                permission,
                User.GetUserEmail());

            var hasPermission = permission.ToLowerInvariant() switch
            {
                "manage-users" => User.CanManageUsers(),
                "manage-roles" => User.CanManageRoles(),
                "manage-games" => User.CanManageGames(),
                "manage-orders" => User.CanManageOrders(),
                "moderate-comments" => User.CanModerateComments(),
                "view-deleted-games" => User.CanViewDeletedGames(),
                _ => false,
            };

            return Ok(new
            {
                permission,
                hasPermission,
                userRole = User.GetUserRole(),
                checkedAt = DateTime.UtcNow,
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error checking permission: {permission}");
        }
    }

    /// <summary>
    /// E12 US1 - Get all available notification methods
    /// Returns a list of supported notification methods for the system.
    /// Public endpoint - no authentication required.
    /// </summary>
    /// <returns>List of available notification methods: ["sms", "push", "email"]. </returns>
    /// <response code="200">Successfully retrieved available notification methods. </response>
    [HttpGet("notifications")]
    [AllowAnonymous]
    public async Task<IActionResult> GetNotificationMethods()
    {
        try
        {
            _logger.LogInformation("Retrieving available notification methods");

            var methods = await _notificationService.GetAvailableNotificationMethodsAsync();

            _logger.LogInformation("Retrieved {Count} available notification methods", methods.Count);

            // Return as array for simplicity (matches Epic 12 specification)
            return Ok(methods);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving available notification methods");
        }
    }

    /// <summary>
    /// E12 US2 - Get current user's notification method preferences
    /// Returns the notification methods that the authenticated user has enabled.
    /// Requires JWT authentication.
    /// </summary>
    /// <returns>User's notification preference data including selected and available methods. </returns>
    /// <response code="200">Successfully retrieved user notification preferences. </response>
    /// <response code="401">Unauthorized - JWT token missing or invalid. </response>
    /// <response code="404">User not found. </response>
    [HttpGet("users/my/notifications")]
    [Authorize]
    public async Task<IActionResult> GetMyNotificationMethods()
    {
        try
        {
            var userEmail = User.GetUserEmail();
            _logger.LogInformation("Retrieving notification preferences for user: {Email}", userEmail);

            // Get all users and find current user by email
            var users = await _userManagementService.GetAllUsersForManagementAsync();
            var currentUser = users.FirstOrDefault(u => u.Name == userEmail);

            if (currentUser == null)
            {
                _logger.LogWarning("User not found for email: {Email}", userEmail);
                return NotFound(new ErrorResponseModel
                {
                    Message = $"User with email {userEmail} not found",
                    StatusCode = StatusCodes.Status404NotFound,
                    ErrorId = Guid.NewGuid().ToString(),
                });
            }

            // Extract userId from UserDto.Id
            if (!Guid.TryParse(currentUser.Id, out var userId))
            {
                _logger.LogError("Invalid user ID format: {UserId}", currentUser.Id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseModel
                {
                    Message = "Invalid user ID format",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ErrorId = Guid.NewGuid().ToString(),
                });
            }

            // Retrieve user's selected notification methods
            var selectedMethods = await _notificationService.GetUserNotificationMethodsAsync(userId);
            var availableMethods = await _notificationService.GetAvailableNotificationMethodsAsync();

            _logger.LogInformation(
                "Retrieved {Count} notification preferences for user: {Email}",
                selectedMethods.Count,
                userEmail);

            return Ok(selectedMethods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user notification preferences");
            return Ok(new List<string>());
        }
    }

    /// <summary>
    /// E12 US3 - Update current user's notification method preferences
    /// Allows authenticated user to select which notification methods to receive.
    /// Requires JWT authentication.
    /// </summary>
    /// <param name="request">Request containing list of notification methods to enable. </param>
    /// <returns>Updated user notification preference data. </returns>
    /// <response code="200">Successfully updated notification preferences. </response>
    /// <response code="400">Bad request - invalid notification methods or empty list. </response>
    /// <response code="401">Unauthorized - JWT token missing or invalid. </response>
    /// <response code="404">User not found. </response>
    [HttpPut("users/notifications")]
    [Authorize]
    public async Task<IActionResult> UpdateNotificationMethods(
        [FromBody] UpdateUserNotificationMethodsRequestDto request)
    {
        try
        {
            var userEmail = User.GetUserEmail();
            _logger.LogInformation(
                "Updating notification preferences for user: {Email}, Methods: {Methods}",
                userEmail,
                string.Join(", ", request.Notifications));

            // Get all users and find current user by email
            var users = await _userManagementService.GetAllUsersForManagementAsync();
            var currentUser = users.FirstOrDefault(u => u.Name == userEmail);

            if (currentUser == null)
            {
                _logger.LogWarning("User not found for email: {Email}", userEmail);
                return NotFound(new ErrorResponseModel
                {
                    Message = $"User with email {userEmail} not found",
                    StatusCode = StatusCodes.Status404NotFound,
                    ErrorId = Guid.NewGuid().ToString(),
                });
            }

            // Extract userId from UserDto.Id
            if (!Guid.TryParse(currentUser.Id, out var userId))
            {
                _logger.LogError("Invalid user ID format: {UserId}", currentUser.Id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseModel
                {
                    Message = "Invalid user ID format",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ErrorId = Guid.NewGuid().ToString(),
                });
            }

            // Update notification preferences
            var result = await _notificationService.UpdateUserNotificationMethodsAsync(
                userId,
                request.Notifications);

            _logger.LogInformation(
                "Successfully updated notification preferences for user: {Email}. New methods: {Methods}",
                userEmail,
                string.Join(", ", result.SelectedMethods));

            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error updating notification preferences");
        }
    }

    /// <summary>
    /// Test email sender.
    /// </summary>
    [HttpPost("test-email")]
    [AllowAnonymous]
    public async Task<IActionResult> TestEmail(string email = "test@example.com")
    {
        var sent = await _emailService.SendEmailAsync(
            email,
            "Test Email from Game Store",
            "<h1>Test</h1><p>If you see this, email works! ✅</p>",
            isHtmlBody: true);

        return Ok(new { success = sent, message = sent ? "Email sent!" : "Failed to send email" });
    }

    /// <summary>
    /// Handles exceptions and maps them to appropriate HTTP responses.
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
                ErrorId = Guid.NewGuid().ToString(),
            }),

            // Not found errors -> Not Found (404)
            KeyNotFoundException => NotFound(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status404NotFound,
                ErrorId = Guid.NewGuid().ToString(),
            }),

            // Conflict errors (already exists, self-deletion) -> Conflict (409)
            InvalidOperationException when ex.Message.Contains("already exists") => Conflict(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status409Conflict,
                ErrorId = Guid.NewGuid().ToString(),
            }),

            // Other business logic errors (self-deletion, etc.) -> Bad Request (400)
            InvalidOperationException => BadRequest(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status400BadRequest,
                ErrorId = Guid.NewGuid().ToString(),
            }),

            // Unexpected errors -> Internal Server Error (500)
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseModel
            {
                Message = "An unexpected error occurred while processing the request.",
                Details = ex.Message,
                StatusCode = StatusCodes.Status500InternalServerError,
                ErrorId = Guid.NewGuid().ToString(),
            }),
        };
    }
}