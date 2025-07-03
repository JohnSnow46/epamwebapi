namespace Gamestore.Services.Dto.FiltersDto;
public static class GameFilterOptions
{
    public static readonly IReadOnlyList<string> PaginationOptions = new List<string>
    {
        "10", "20", "50", "100", "all"
    }.AsReadOnly();

    public static readonly IReadOnlyList<string> SortingOptions = new List<string>
    {
        "Most popular",
        "Most commented",
        "Price ASC",
        "Price DESC",
        "New"
    }.AsReadOnly();

    public static readonly IReadOnlyList<string> PublishDateOptions = new List<string>
    {
        "last week",
        "last month",
        "last year",
        "2 years",
        "3 years"
    }.AsReadOnly();

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