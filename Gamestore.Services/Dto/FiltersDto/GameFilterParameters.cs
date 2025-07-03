using Microsoft.AspNetCore.Mvc;

namespace Gamestore.Services.Dto.FiltersDto;
public class GameFilterParameters
{
    [FromQuery(Name = "genres")]
    public List<Guid>? GenreIds { get; set; }

    [FromQuery(Name = "platforms")]
    public List<Guid>? PlatformIds { get; set; }

    [FromQuery(Name = "publishers")]
    public List<Guid>? PublisherIds { get; set; }

    [FromQuery(Name = "minPrice")]
    public double? MinPrice { get; set; }

    [FromQuery(Name = "maxPrice")]
    public double? MaxPrice { get; set; }

    [FromQuery(Name = "datePublishing")]
    public string? PublishDateFilter { get; set; }

    [FromQuery(Name = "name")]
    public string? Name { get; set; }

    [FromQuery(Name = "sort")]
    public string? SortBy { get; set; } = "New";

    [FromQuery(Name = "pageCount")]
    public string? PageSize { get; set; } = "10";

    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;
}
