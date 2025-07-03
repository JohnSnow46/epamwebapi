using Gamestore.Services.Dto.GenresDto;

namespace Gamestore.Services.Interfaces;

public interface IGenreService
{
    Task<GenreUpdateRequestDto> GetGenreById(Guid id);

    Task<GenreUpdateRequestDto> UpdateGenre(Guid id, GenreMetadataUpdateRequestDto genreRequest);

    Task<GenreCreateRequestDto> CreateGenre(GenreCreateRequestDto genreRequest);

    Task<IEnumerable<GenreUpdateRequestDto>> GetAllGenres();

    Task<IEnumerable<GenreUpdateRequestDto>> GetSubGenresAsync(Guid id);

    Task<GenreUpdateRequestDto> DeleteGenreById(Guid id);

    Task<IEnumerable<GenreUpdateRequestDto>> GetGenresByGameKeyAsync(string gameKey);
}