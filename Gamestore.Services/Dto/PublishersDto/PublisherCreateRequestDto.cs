namespace Gamestore.Services.Dto.PublishersDto;

/// <summary>
/// Represents a data transfer object for creating a new publisher in the game store system.
/// Used to provide publisher information required for creating publisher records.
/// </summary>
public class PublisherCreateRequestDto
{
    /// <summary>
    /// Gets or sets the publisher metadata containing detailed information about the publisher.
    /// This includes all the necessary information to create a publisher record.
    /// </summary>
    public PublisherMetadataCreateRequestDto Publisher { get; set; }
}
