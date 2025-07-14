namespace Gamestore.Services.Dto.FiltersDto;

/// <summary>
/// Provides static configuration options for game filtering and sorting in the game store system.
/// Contains predefined sets of filter options and utility methods for date-based filtering.
/// </summary>
public static class GameFilterOptions
{
    /// <summary>
    /// Gets the available pagination options for displaying games per page.
    /// Includes standard page sizes and an "all" option to display all games.
    /// </summary>
    public static readonly IReadOnlyList<string> PaginationOptions = new List<string>
    {
        "10", "20", "50", "100", "all"
    }.AsReadOnly();

    /// <summary>
    /// Gets the available sorting options for game listings.
    /// Includes popularity, comments, price, and release date sorting options.
    /// </summary>
    public static readonly IReadOnlyList<string> SortingOptions = new List<string>
    {
        "Most popular",
        "Most commented",
        "Price ASC",
        "Price DESC",
        "New"
    }.AsReadOnly();

    /// <summary>
    /// Gets the available publish date filter options for games.
    /// Includes predefined time ranges for filtering games by their publication date.
    /// </summary>
    public static readonly IReadOnlyList<string> PublishDateOptions = new List<string>
    {
        "last week",
        "last month",
        "last year",
        "2 years",
        "3 years"
    }.AsReadOnly();

    /// <summary>
    /// Converts a date filter string to a DateTime value for filtering games by publication date.
    /// </summary>
    /// <param name="filter">The date filter string (e.g., "last week", "last month").</param>
    /// <returns>A DateTime representing the cutoff date for the filter, or DateTime.MinValue if the filter is invalid or null.</returns>
    public static DateTime GetDateFromFilter(string? filter)
    {
        return string.IsNullOrWhiteSpace(filter)
            ? DateTime.MinValue
            : filter switch
            {
                "last week" => DateTime.UtcNow.AddDays(-7),
                "last month" => DateTime.UtcNow.AddMonths(-1),
                "last year" => DateTime.UtcNow.AddYears(-1),
                "2 years" => DateTime.UtcNow.AddYears(-2),
                "3 years" => DateTime.UtcNow.AddYears(-3),
                _ => DateTime.MinValue
            };
    }
}