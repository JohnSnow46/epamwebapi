using System.Text.Json.Serialization;

namespace Gamestore.Entities.ErrorModels;

/// <summary>
/// Represents a standardized error response model for API error handling and client communication.
/// Provides comprehensive error information including tracking identifiers, user-friendly messages,
/// HTTP status codes, and optional retry guidance for consistent error response formatting.
/// </summary>
public class ErrorResponseModel
{
    /// <summary>
    /// Gets or sets the unique identifier for this specific error occurrence.
    /// This is automatically generated for each error instance and can be used for
    /// error tracking, logging correlation, and support ticket references.
    /// </summary>
    public string ErrorId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the user-friendly error message describing what went wrong.
    /// This should be a clear, actionable message that can be displayed to end users.
    /// Defaults to a generic error message if no specific message is provided.
    /// </summary>
    public string Message { get; set; } = "An unexpected error occurred.";

    /// <summary>
    /// Gets or sets the HTTP status code associated with this error.
    /// This corresponds to standard HTTP status codes (400, 401, 404, 500, etc.)
    /// and helps clients understand the nature and severity of the error.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the optional timestamp indicating when the client should retry the request.
    /// This is used for rate limiting, temporary service unavailability, or throttling scenarios.
    /// Only included in the JSON response when a value is set, otherwise omitted entirely.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? RetryAfter { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this error occurred.
    /// This is automatically set to the current UTC time when the error response is created,
    /// providing temporal context for error analysis and debugging.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets additional detailed error information for debugging or advanced error handling.
    /// This may include stack traces, validation errors, or other technical details.
    /// Only included in the JSON response when a value is set, otherwise omitted entirely.
    /// Should be used carefully to avoid exposing sensitive system information.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Details { get; set; }
}