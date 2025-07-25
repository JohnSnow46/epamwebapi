using System.Text.Json;
using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Dto.AuthDto;
using Gamestore.Services.Interfaces;
using Gamestore.Services.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.Auth;

/// <summary>
/// Controller for authentication operations.
/// Uses IDatabaseRoleService for core authentication and system operations.
/// Supports both internal (database) and external (microservice) authentication.
/// </summary>
[ApiController]
[Route("api")]
public class AuthController(
    IDatabaseRoleService databaseRoleService,
    ILogger<AuthController> logger,
    IWebHostEnvironment environment,
    IServiceProvider serviceProvider) : ControllerBase
{
    private readonly IDatabaseRoleService _databaseRoleService = databaseRoleService;
    private readonly ILogger<AuthController> _logger = logger;
    private readonly IWebHostEnvironment _environment = environment;
    private readonly AuthService _authService = serviceProvider.GetRequiredService<AuthService>();

    /// <summary>
    /// Epic 9 US1 - Login endpoint with database authentication and fallback to external
    /// </summary>
    [HttpPost("users/login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] JsonElement requestBody)
    {
        try
        {
            _logger.LogInformation("🔍 Received login request");

            var (email, password, internalAuth) = ParseLoginRequest(requestBody);

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Email and password are required",
                    StatusCode = StatusCodes.Status400BadRequest,
                    ErrorId = Guid.NewGuid().ToString()
                });
            }

            _logger.LogInformation("🔍 Login attempt for: {Email}, InternalAuth: {InternalAuth}", email, internalAuth);

            return internalAuth
                ? await HandleDatabaseAuthentication(email, password)
                : await HandleExternalAuthentication(email, password);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error during login");
        }
    }

    /// <summary>
    /// Epic 9 US2 - Check page access endpoint with database role checking
    /// </summary>
    [HttpPost("users/access")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckAccess([FromBody] AccessCheckRequest request)
    {
        try
        {
            _logger.LogInformation("Checking access for page: {Page}, ID: {Id}", request.TargetPage, request.TargetId);

            var userRole = await GetUserRoleAsync();
            var hasAccess = CheckPageAccess(request.TargetPage, userRole);

            var result = new
            {
                access = hasAccess,
                message = hasAccess ? "Access granted" : "Access denied",
                userRole,
                targetPage = request.TargetPage,
                targetId = request.TargetId,
                checkedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Access check result: {HasAccess} for user role: {Role} on page: {Page}",
                hasAccess, userRole, request.TargetPage);

            return hasAccess ? Ok(result) : Forbid();
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error checking access");
        }
    }

    /// <summary>
    /// Register new user endpoint - Epic 9 extension
    /// </summary>
    [HttpPost("users/register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] JsonElement requestBody)
    {
        try
        {
            _logger.LogInformation("🔍 Received registration request");

            var (email, password, firstName, lastName) = ParseRegistrationRequest(requestBody);

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Email, password, firstName, and lastName are required",
                    StatusCode = StatusCodes.Status400BadRequest,
                    ErrorId = Guid.NewGuid().ToString()
                });
            }

            if (await _databaseRoleService.UserExistsAsync(email))
            {
                return Conflict(new ErrorResponseModel
                {
                    Message = "User with this email already exists",
                    StatusCode = StatusCodes.Status409Conflict,
                    ErrorId = Guid.NewGuid().ToString()
                });
            }

            var passwordHash = HashPassword(password);
            var newUser = await _databaseRoleService.CreateUserAsync(email, firstName, lastName, passwordHash);

            _logger.LogInformation("✅ User registered successfully: {Email}", email);

            return Ok(new
            {
                success = true,
                message = "User registered successfully",
                user = new
                {
                    id = newUser.Id,
                    email = newUser.Email,
                    name = newUser.FullName,
                    role = await _databaseRoleService.GetUserRoleAsync(email)
                },
                registeredAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error during registration");
        }
    }

    /// <summary>
    /// Get current user profile information
    /// </summary>
    [HttpGet("users/profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userEmail = User.FindFirst("email")?.Value ??
                           User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Unable to identify current user",
                    StatusCode = StatusCodes.Status400BadRequest,
                    ErrorId = Guid.NewGuid().ToString()
                });
            }

            var user = await _databaseRoleService.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound(new ErrorResponseModel
                {
                    Message = "User profile not found",
                    StatusCode = StatusCodes.Status404NotFound,
                    ErrorId = Guid.NewGuid().ToString()
                });
            }

            var userRole = await _databaseRoleService.GetUserRoleAsync(userEmail);
            var userRoles = await _databaseRoleService.GetUserRolesAsync(userEmail);

            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                fullName = user.FullName,
                isActive = user.IsActive,
                role = userRole,
                roles = userRoles,
                isEmailConfirmed = user.IsEmailConfirmed,
                createdAt = user.CreatedAt,
                lastLoginAt = user.LastLoginAt
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving user profile");
        }
    }

    #region Private Authentication Methods

    /// <summary>
    /// Handle database authentication using the DatabaseRoleService
    /// </summary>
    private async Task<IActionResult> HandleDatabaseAuthentication(string email, string password)
    {
        try
        {
            _logger.LogInformation("🏠 Processing database authentication for: {Email}", email);

            if (!await _databaseRoleService.ValidateUserPasswordAsync(email, password))
            {
                _logger.LogWarning("❌ Database auth failed for user: {Email}", email);
                return Unauthorized(new ErrorResponseModel
                {
                    Message = "Invalid credentials",
                    StatusCode = StatusCodes.Status401Unauthorized,
                    ErrorId = Guid.NewGuid().ToString()
                });
            }

            var user = await _databaseRoleService.GetUserByEmailAsync(email);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("❌ User not found or inactive: {Email}", email);
                return Unauthorized(new ErrorResponseModel
                {
                    Message = "Invalid credentials or account inactive",
                    StatusCode = StatusCodes.Status401Unauthorized,
                    ErrorId = Guid.NewGuid().ToString()
                });
            }

            var userRole = await _databaseRoleService.GetUserRoleAsync(email);
            await _databaseRoleService.UpdateUserAsync(user.Id);

            var token = JwtHelper.GenerateToken(user.Email, user.FirstName, user.LastName, userRole, user.Id);

            _logger.LogInformation("✅ Database auth successful for: {Email}, Role: {Role}", email, userRole);

            return Ok(new
            {
                token,
                success = true,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    name = user.FullName,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    role = userRole,
                    roles = user.GetRoleNames(),
                    isAuthenticated = true,
                    authMethod = "Database",
                    lastLogin = user.LastLoginAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database authentication for: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Handle external authentication using AuthService microservice (fallback)
    /// </summary>
    private async Task<IActionResult> HandleExternalAuthentication(string email, string password)
    {
        try
        {
            _logger.LogInformation("🌐 Processing external authentication for: {Email}", email);

            if (_authService == null)
            {
                _logger.LogWarning("🚫 External AuthService not configured but requested for user: {Email}", email);
                return BadRequest(new ErrorResponseModel
                {
                    Message = "External authentication service is not configured. Please contact administrator or use internal authentication.",
                    StatusCode = StatusCodes.Status400BadRequest,
                    ErrorId = Guid.NewGuid().ToString()
                });
            }

            var authResult = await _authService.AuthenticateAsync(email, password);

            if (authResult == null)
            {
                _logger.LogWarning("❌ External auth failed for user: {Email}", email);
                return Unauthorized(new ErrorResponseModel
                {
                    Message = "Invalid credentials",
                    StatusCode = StatusCodes.Status401Unauthorized,
                    ErrorId = Guid.NewGuid().ToString()
                });
            }

            var existingUser = await _databaseRoleService.GetUserByEmailAsync(authResult.Email);
            if (existingUser == null)
            {
                _logger.LogInformation("Creating new user from external auth: {Email}", authResult.Email);

                var randomHash = Guid.NewGuid().ToString();
                existingUser = await _databaseRoleService.CreateUserAsync(
                    authResult.Email,
                    authResult.FirstName,
                    authResult.LastName,
                    randomHash,
                    Roles.User
                );
            }

            var userRole = await _databaseRoleService.GetUserRoleAsync(authResult.Email);
            var token = JwtHelper.GenerateToken(
                authResult.Email,
                authResult.FirstName,
                authResult.LastName,
                userRole,
                existingUser.Id);

            _logger.LogInformation("✅ External auth successful for: {Email}, Role: {Role}", authResult.Email, userRole);

            return Ok(new
            {
                token,
                success = true,
                user = new
                {
                    id = existingUser.Id,
                    email = authResult.Email,
                    name = $"{authResult.FirstName} {authResult.LastName}",
                    firstName = authResult.FirstName,
                    lastName = authResult.LastName,
                    role = userRole,
                    isAuthenticated = true,
                    authMethod = "External"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during external authentication for: {Email}", email);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseModel
            {
                Message = "External authentication service error. Please try again or use internal authentication.",
                StatusCode = StatusCodes.Status500InternalServerError,
                ErrorId = Guid.NewGuid().ToString()
            });
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Gets user role
    /// </summary>
    private async Task<string> GetUserRoleAsync()
    {
        var userEmail = User.FindFirst("email")?.Value ??
                       User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        if (!string.IsNullOrEmpty(userEmail))
        {
            return await _databaseRoleService.GetUserRoleAsync(userEmail);
        }

        // Fallback to token claims if no email found
        return User.FindFirst("role")?.Value ??
               User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ??
               Roles.Guest;
    }

    /// <summary>
    /// Checks if user has access to specific page
    /// </summary>
    private static bool CheckPageAccess(string targetPage, string userRole)
    {
        return targetPage.ToLowerInvariant() switch
        {
            "admin" => userRole == Roles.Administrator,
            "usermanagement" => userRole == Roles.Administrator,
            "rolemanagement" => userRole == Roles.Administrator,
            "ordermanagement" => Roles.HasPermission(userRole, Roles.Manager),
            "commentmoderation" => Roles.HasPermission(userRole, Roles.Moderator),
            "gamemanagement" => Roles.HasPermission(userRole, Roles.Manager),
            "genre" => true, // Everyone can access
            "platform" => true, // Everyone can access
            "publisher" => true, // Everyone can access
            "game" => true, // Everyone can access
            "games" => true, // Everyone can access
            "genres" => true, // Everyone can access
            "platforms" => true, // Everyone can access
            "publishers" => true, // Everyone can access
            "history" => true, // Everyone can access order history
            "orders" => true, // Everyone can access orders
            "order" => true, // Everyone can access individual orders
            "basket" => true, // Everyone can access basket
            "makeorder" => true, // Everyone can access make order
            "users" => userRole == Roles.Administrator, // Only admin can manage users
            "roles" => userRole == Roles.Administrator, // Only admin can manage roles
            _ => true // Default allow for basic pages
        };
    }

    /// <summary>
    /// Parse login request from various formats
    /// </summary>
    private static (string email, string password, bool internalAuth) ParseLoginRequest(JsonElement jsonElement)
    {
        string email = string.Empty;
        string password = string.Empty;
        bool internalAuth = true;

        // Parse different request formats for compatibility
        if (jsonElement.TryGetProperty("model", out var modelProperty))
        {
            email = modelProperty.GetProperty("login").GetString() ?? string.Empty;
            password = modelProperty.GetProperty("password").GetString() ?? string.Empty;
            if (modelProperty.TryGetProperty("internalAuth", out var internalAuthProp))
            {
                internalAuth = internalAuthProp.GetBoolean();
            }
        }
        else if (jsonElement.TryGetProperty("login", out var loginProp))
        {
            email = loginProp.GetString() ?? string.Empty;
            password = jsonElement.GetProperty("password").GetString() ?? string.Empty;
            if (jsonElement.TryGetProperty("internalAuth", out var directInternalAuthProp))
            {
                internalAuth = directInternalAuthProp.GetBoolean();
            }
        }
        else if (jsonElement.TryGetProperty("email", out var emailProp))
        {
            email = emailProp.GetString() ?? string.Empty;
            password = jsonElement.GetProperty("password").GetString() ?? string.Empty;
            if (jsonElement.TryGetProperty("internalAuth", out var emailInternalAuthProp))
            {
                internalAuth = emailInternalAuthProp.GetBoolean();
            }
        }

        return (email, password, internalAuth);
    }

    /// <summary>
    /// Parse registration request
    /// </summary>
    private static (string email, string password, string firstName, string lastName) ParseRegistrationRequest(JsonElement jsonElement)
    {
        var email = jsonElement.TryGetProperty("email", out var emailProp) ? emailProp.GetString() ?? string.Empty : string.Empty;
        var password = jsonElement.TryGetProperty("password", out var passwordProp) ? passwordProp.GetString() ?? string.Empty : string.Empty;
        var firstName = jsonElement.TryGetProperty("firstName", out var firstNameProp) ? firstNameProp.GetString() ?? string.Empty : string.Empty;
        var lastName = jsonElement.TryGetProperty("lastName", out var lastNameProp) ? lastNameProp.GetString() ?? string.Empty : string.Empty;

        return (email, password, firstName, lastName);
    }

    /// <summary>
    /// Hash password using simple SHA256 (in production use BCrypt)
    /// </summary>
    private static string HashPassword(string password)
    {
        var hashedBytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password + "GamestoreSalt2024"));
        return Convert.ToBase64String(hashedBytes);
    }

    /// <summary>
    /// Handles exceptions and returns appropriate error response
    /// </summary>
    private ObjectResult HandleException(Exception ex, string logMessage)
    {
        _logger.LogError(ex, "{LogMessage}: {ErrorMessage}", logMessage, ex.Message);

        return ex switch
        {
            ArgumentException => BadRequest(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status400BadRequest,
                ErrorId = Guid.NewGuid().ToString()
            }),
            KeyNotFoundException => NotFound(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status404NotFound,
                ErrorId = Guid.NewGuid().ToString()
            }),
            InvalidOperationException when ex.Message.Contains("already exists") => Conflict(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status409Conflict,
                ErrorId = Guid.NewGuid().ToString()
            }),
            InvalidOperationException => BadRequest(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status400BadRequest,
                ErrorId = Guid.NewGuid().ToString()
            }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseModel
            {
                Message = "An error occurred during authentication.",
                Details = _environment.IsDevelopment() ? ex.Message : string.Empty,
                StatusCode = StatusCodes.Status500InternalServerError,
                ErrorId = Guid.NewGuid().ToString()
            })
        };
    }

    #endregion
}