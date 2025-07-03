namespace Gamestore.Services.Dto.GamesDto;
public class GameMetadataCreateRequestDto
{
    public GameCreateRequestDto Game { get; set; } = new();

    public Guid Publisher { get; set; }

    public List<Guid>? Genres { get; set; }

    public List<Guid>? Platforms { get; set; }
}
