using Gamestore.Services.Dto.GamesDto;

namespace Gamestore.Services.Dto.FiltersDto;
public class GameFilterResult
{
    public List<GameUpdateRequestDto> Games { get; set; } = new();

    public int TotalPages { get; set; }

    public int CurrentPage { get; set; }
}