using Microsoft.AspNetCore.Mvc;

namespace Gamestore.Services.Dto.FiltersDto;

/// <summary>
/// Represents query parameters for filtering and sorting games in the game store system.
/// Used to bind query string parameters from HTTP requests to provide flexible game filtering options.
/// </summary>
public class GameFilterParameters
{
    /// <summary>
    /// Gets or sets the list of genre identifiers to filter games by.
    /// This field is optional and allows filtering games by one or more genres.
    /// </summary>
    [FromQuery(Name = "genres")]
    public List<Guid>? GenreIds { get; set; }

    /// <summary>
    /// Gets or sets the list of platform identifiers to filter games by.
    /// This field is optional and allows filtering games by one or more platforms.
    /// </summary>
    [FromQuery(Name = "platforms")]
    public List<Guid>? PlatformIds { get; set; }

    /// <summary>
    /// Gets or sets the list of publisher identifiers to filter games by.
    /// This field is optional and allows filtering games by one or more publishers.
    /// </summary>
    [FromQuery(Name = "publishers")]
    public List<Guid>? PublisherIds { get; set; }

    /// <summary>
    /// Gets or sets the minimum price threshold for filtering games.
    /// This field is optional and allows filtering games with prices greater than or equal to this value.
    /// </summary>
    [FromQuery(Name = "minPrice")]
    public double? MinPrice { get; set; }

    /// <summary>
    /// Gets or sets the maximum price threshold for filtering games.
    /// This field is optional and allows filtering games with prices less than or equal to this value.
    /// </summary>
    [FromQuery(Name = "maxPrice")]
    public double? MaxPrice { get; set; }

    /// <summary>
    /// Gets or sets the publish date filter for games.
    /// This field is optional and allows filtering games by their publication date using predefined time ranges.
    /// </summary>
    [FromQuery(Name = "datePublishing")]
    public string? PublishDateFilter { get; set; }

    /// <summary>
    /// Gets or sets the name filter for searching games by name.
    /// This field is optional and allows searching games by their title or name.
    /// </summary>
    [FromQuery(Name = "name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the sorting criteria for the game results.
    /// This field is optional and defaults to "New" sorting. Should match one of the available sorting options.
    /// </summary>
    [FromQuery(Name = "sort")]
    public string? SortBy { get; set; } = "New";

    /// <summary>
    /// Gets or sets the page size for pagination.
    /// This field is optional and defaults to "10". Determines how many games to display per page.
    /// </summary>
    [FromQuery(Name = "pageCount")]
    public string? PageSize { get; set; } = "10";

    /// <summary>
    /// Gets or sets the page number for pagination.
    /// This field defaults to 1 and determines which page of results to retrieve.
    /// </summary>
    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;
}
