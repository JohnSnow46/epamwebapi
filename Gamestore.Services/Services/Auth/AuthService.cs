using Gamestore.Services.Dto.AuthDto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Gamestore.Services.Services.Auth;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthService> _logger;
    private readonly string _authServiceBaseUrl;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthService(HttpClient httpClient, ILogger<AuthService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;

        _authServiceBaseUrl = configuration["AuthService:BaseUrl"] ?? "https://localhost:5037";

        if (_httpClient.BaseAddress == null)
        {
            _logger.LogWarning("⚠️ HttpClient BaseAddress is null, setting manually to: {BaseUrl}", _authServiceBaseUrl);
            _httpClient.BaseAddress = new Uri(_authServiceBaseUrl);
        }

        _logger.LogInformation("✅ AuthService initialized with BaseAddress: {BaseAddress}", _httpClient.BaseAddress);
    }

    public async Task<AuthResponseDto?> AuthenticateAsync(string email, string password)
    {
        _logger.LogInformation("🔍 AuthService.AuthenticateAsync called for: {Email}", email);
        _logger.LogInformation("🔍 HttpClient BaseAddress: {BaseAddress}", _httpClient.BaseAddress);

        if (_httpClient.BaseAddress == null)
        {
            _logger.LogError("❌ HttpClient BaseAddress is still null!");
            _httpClient.BaseAddress = new Uri(_authServiceBaseUrl);
            _logger.LogInformation("🔧 Set BaseAddress manually to: {BaseUrl}", _authServiceBaseUrl);
        }

        var authRequest = new
        {
            email = email,
            password = password
        };

        var json = JsonSerializer.Serialize(authRequest);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        _logger.LogInformation("🔍 Request payload: {Json}", json);
        _logger.LogInformation("🔍 Making request to: {BaseAddress}/api/auth", _httpClient.BaseAddress);

        var response = await _httpClient.PostAsync("/api/auth", content);

        _logger.LogInformation("🔍 AuthService response status: {StatusCode}", response.StatusCode);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("🔍 AuthService response content: {Content}", responseContent);

            var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(responseContent, JsonOptions);

            _logger.LogInformation("✅ AuthService authentication successful for user: {Email}", email);
            return authResponse;
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        _logger.LogWarning("❌ AuthService authentication failed for user: {Email}. Status: {StatusCode}, Error: {Error}",
            email, response.StatusCode, errorContent);

        // Throw exception instead of returning null - let controller handle it
        throw new UnauthorizedAccessException($"Authentication failed for user {email}: {errorContent}");
    }

    public async Task<IEnumerable<AuthResponseDto>> GetUsersAsync()
    {
        _logger.LogInformation("Getting users from AuthService");

        var response = await _httpClient.GetAsync("/api/users");

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<IEnumerable<AuthResponseDto>>(responseContent, JsonOptions);

            return users ?? Enumerable.Empty<AuthResponseDto>();
        }

        _logger.LogWarning("Failed to get users from AuthService. Status: {StatusCode}", response.StatusCode);

        // Throw exception instead of returning empty collection
        throw new HttpRequestException($"Failed to get users from AuthService. Status: {response.StatusCode}");
    }

    public async Task<bool> CreateUserAsync(string email, string firstName, string lastName, string password)
    {
        var createUserRequest = new
        {
            email = email,
            firstName = firstName,
            lastName = lastName,
            password = password,
            confirmPassword = password
        };

        var json = JsonSerializer.Serialize(createUserRequest);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        _logger.LogInformation("Creating user in AuthService: {Email}", email);

        var response = await _httpClient.PostAsync("/api/users", content);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("User created successfully in AuthService: {Email}", email);
            return true;
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        _logger.LogWarning("Failed to create user in AuthService: {Email}. Status: {StatusCode}, Error: {Error}",
            email, response.StatusCode, errorContent);

        // Throw exception instead of returning false
        throw new InvalidOperationException($"Failed to create user {email}: {errorContent}");
    }
}