using Gamestore.Services.Dto.GamesDto;

namespace Gamestore.Services.Dto.FiltersDto;

/// <summary>
/// Represents the result of a game filtering operation in the game store system.
/// Contains the filtered games along with pagination information for displaying results across multiple pages.
/// </summary>
public class GameFilterResult
{
    /// <summary>
    /// Gets or sets the list of games that match the applied filters.
    /// Contains the game data for the current page of results.
    /// </summary>
    public List<GameUpdateRequestDto> Games { get; set; } = new();

    /// <summary>
    /// Gets or sets the total number of pages available for the filtered results.
    /// Used for pagination controls and navigation.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Gets or sets the current page number being displayed.
    /// Represents the page position within the total set of paginated results.
    /// </summary>
    public int CurrentPage { get; set; }
}