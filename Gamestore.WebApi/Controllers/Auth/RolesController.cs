using Gamestore.Entities.ErrorModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Gamestore.Services.Services.Auth;
using Gamestore.Services.Dto.AuthDto;
using Gamestore.WebApi.Extensions;
using Gamestore.Services.Interfaces;

namespace Gamestore.WebApi.Controllers.Auth;

/// <summary>
/// Controller for role management operations.
/// Uses IRoleManagementService for all management operations.
/// Only administrators can access these endpoints.
/// </summary>
[ApiController]
[Route("api")]
public class RolesController(IRoleManagementService roleManagementService, ILogger<RolesController> logger) : ControllerBase
{
    private readonly IRoleManagementService _roleManagementService = roleManagementService;
    private readonly ILogger<RolesController> _logger = logger;

    /// <summary>
    /// US6 - Get all roles endpoint
    /// Epic 9: Only Admin can manage roles
    /// </summary>
    [HttpGet("roles")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            _logger.LogInformation("Getting all roles by admin: {Admin}", User.GetUserEmail());

            var roles = await _roleManagementService.GetAllRolesForManagementAsync();

            _logger.LogInformation("Retrieved {Count} roles", roles.Count());
            return Ok(roles);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving all roles");
        }
    }

    /// <summary>
    /// US7 - Get role by id endpoint
    /// Epic 9: Only Admin can view specific role details
    /// </summary>
    [HttpGet("roles/{id}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> GetRoleById(string id)
    {
        try
        {
            _logger.LogInformation("Getting role by ID: {RoleId} by admin: {Admin}", id, User.GetUserEmail());

            var role = await _roleManagementService.GetRoleForManagementAsync(id);

            _logger.LogInformation("Found role with ID: {RoleId}", id);
            return Ok(role);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error retrieving role with ID: {id}");
        }
    }

    /// <summary>
    /// US8 - Delete role by id endpoint
    /// Epic 9: Only Admin can delete roles
    /// </summary>
    [HttpDelete("roles/{id}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> DeleteRoleById(string id)
    {
        try
        {
            _logger.LogInformation("Deleting role with ID: {RoleId} by admin: {Admin}", id, User.GetUserEmail());

            var result = await _roleManagementService.DeleteRoleForManagementAsync(id);

            _logger.LogInformation("Successfully deleted role with ID: {RoleId}", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error deleting role with ID: {id}");
        }
    }

    /// <summary>
    /// US12 - Get permissions endpoint
    /// Epic 9: Only Admin can view all permissions
    /// </summary>
    [HttpGet("roles/permissions")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> GetAllPermissions()
    {
        try
        {
            _logger.LogInformation("Getting all permissions by admin: {Admin}", User.GetUserEmail());

            var permissions = await _roleManagementService.GetAllPermissionsForManagementAsync();

            _logger.LogInformation("Retrieved {Count} permissions", permissions.Count());
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving all permissions");
        }
    }

    /// <summary>
    /// US13 - Get role permissions endpoint
    /// Epic 9: Only Admin can view role permissions
    /// </summary>
    [HttpGet("roles/{id}/permissions")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> GetRolePermissions(string id)
    {
        try
        {
            _logger.LogInformation("Getting permissions for role ID: {RoleId} by admin: {Admin}", id, User.GetUserEmail());

            var permissions = await _roleManagementService.GetRolePermissionsForManagementAsync(id);

            _logger.LogInformation("Retrieved {Count} permissions for role: {RoleId}", permissions.Count(), id);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error retrieving permissions for role ID: {id}");
        }
    }

    /// <summary>
    /// US14 - Add role endpoint
    /// Epic 9: Only Admin can create roles
    /// </summary>
    [HttpPost("roles")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> AddRole([FromBody] AddRoleRequest request)
    {
        try
        {
            _logger.LogInformation("Creating new role: {RoleName} by admin: {Admin}",
                request.Role.Name, User.GetUserEmail());

            var result = await _roleManagementService.CreateRoleForManagementAsync(request);

            // Extract ID from result for CreatedAtAction
            var resultDict = result.GetType().GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(result));
            var roleId = resultDict.TryGetValue("id", out var value) ? value?.ToString() : "unknown";

            _logger.LogInformation("Successfully created role with ID: {RoleId}", roleId);
            return CreatedAtAction(nameof(GetRoleById), new { id = roleId }, result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error creating role");
        }
    }

    /// <summary>
    /// US15 - Update role endpoint
    /// Epic 9: Only Admin can update roles
    /// </summary>
    [HttpPut("roles")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequest request)
    {
        try
        {
            _logger.LogInformation("Updating role: {RoleId} by admin: {Admin}",
                request.Role.Id, User.GetUserEmail());

            var result = await _roleManagementService.UpdateRoleForManagementAsync(request);

            _logger.LogInformation("Successfully updated role: {RoleId}", request.Role.Id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error updating role");
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

            // Conflict errors (already exists) -> Conflict (409)
            InvalidOperationException when ex.Message.Contains("already exists") => Conflict(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status409Conflict,
                ErrorId = Guid.NewGuid().ToString()
            }),

            // Other business logic errors -> Bad Request (400)
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