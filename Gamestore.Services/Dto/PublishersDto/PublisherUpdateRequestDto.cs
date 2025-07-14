using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.PublishersDto;

/// <summary>
/// Represents a data transfer object for updating an existing publisher in the game store system.
/// Used to modify publisher information including company details, website, and description.
/// </summary>
public class PublisherUpdateRequestDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the publisher to be updated.
    /// This field is required and must match an existing publisher in the system.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonRequired]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the updated company name of the publisher.
    /// This field is required and should contain the official company name.
    /// </summary>
    [JsonPropertyName("companyName")]
    [JsonRequired]
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the updated homepage URL of the publisher.
    /// This field is optional and should contain the publisher's official website URL.
    /// </summary>
    [JsonPropertyName("homePage")]
    public string? HomePage { get; set; }

    /// <summary>
    /// Gets or sets the updated description of the publisher.
    /// This field is optional and can contain information about the publisher's history, focus, or business model.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}