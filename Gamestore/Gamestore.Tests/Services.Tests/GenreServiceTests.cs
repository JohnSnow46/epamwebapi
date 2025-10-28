using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Gamestore.Services.Dto.GenresDto;
using Gamestore.Services.Services.Business;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.Tests.Services.Tests;

/// <summary>
/// Unit tests for GenreService.
/// </summary>
public class GenreServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<GenreService>> _loggerMock;
    private readonly GenreService _genreService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenreServiceTests"/> class.
    /// </summary>
    public GenreServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<GenreService>>();
        _genreService = new GenreService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Test that GetGenreById returns GenreUpdateRequestDto when genre exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetGenreByIdShouldReturnGenreDtoWhenGenreExists()
    {
        // Arrange
        var genreId = Guid.NewGuid();
        var genre = new Genre
        {
            Id = genreId,
            Name = "Action",
            ParentGenreId = Guid.NewGuid(),
        };

        _unitOfWorkMock
            .Setup(u => u.Genres.GetByIdAsync(genreId))
            .ReturnsAsync(genre);

        // Act
        var result = await _genreService.GetGenreById(genreId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(genreId, result.Id);
        Assert.Equal("Action", result.Name);
        Assert.Equal(genre.ParentGenreId, result.ParentGenreId);
    }

    /// <summary>
    /// Test that GetAllGenres returns GenreUpdateRequestDto collection when genres exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetAllGenresShouldReturnGenreDtosWhenGenresExist()
    {
        // Arrange
        var genres = new List<Genre>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Action",
                ParentGenreId = null,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Adventure",
                ParentGenreId = null,
            },
        };

        _unitOfWorkMock
            .Setup(u => u.Genres.GetAllAsync())
            .ReturnsAsync(genres);

        // Act
        var result = await _genreService.GetAllGenres();
        var resultList = result.ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, g => g.Name == "Action");
        Assert.Contains(resultList, g => g.Name == "Adventure");
    }

    /// <summary>
    /// Test that GetGenresByGameKeyAsync returns GenreUpdateRequestDto collection when game exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetGenresByGameKeyAsyncShouldReturnGenreDtosWhenGameExists()
    {
        // Arrange
        var gameKey = "game123";
        var gameId = Guid.NewGuid();
        var game = new Game { Id = gameId, Key = gameKey };

        var genre1Id = Guid.NewGuid();
        var genre2Id = Guid.NewGuid();

        var gameGenres = new List<GameGenre>
        {
            new() { GameId = gameId, GenreId = genre1Id },
            new() { GameId = gameId, GenreId = genre2Id },
        };

        var genres = new List<Genre>
        {
            new() { Id = genre1Id, Name = "Action" },
            new() { Id = genre2Id, Name = "Adventure" },
        };

        _unitOfWorkMock
            .Setup(u => u.Games.GetKeyAsync(gameKey))
            .ReturnsAsync(game);

        _unitOfWorkMock
            .Setup(u => u.GameGenres.GetByGameIdAsync(gameId))
            .ReturnsAsync(gameGenres);

        _unitOfWorkMock
            .Setup(u => u.GameGenres.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(genres);

        // Act
        var result = await _genreService.GetGenresByGameKeyAsync(gameKey);
        var resultList = result.ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, g => g.Name == "Action");
        Assert.Contains(resultList, g => g.Name == "Adventure");
    }

    /// <summary>
    /// Test that CreateGenre creates new genre and returns GenreCreateRequestDto.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CreateGenreShouldCreateNewGenre()
    {
        // Arrange
        var genreRequest = new GenreCreateRequestDto
        {
            Name = "RPG",
            ParentGenreId = null,
        };

        _unitOfWorkMock
            .Setup(u => u.Genres.GetAllAsync())
            .ReturnsAsync(new List<Genre>());

        _unitOfWorkMock
            .Setup(u => u.Genres.AddAsync(It.IsAny<Genre>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _genreService.CreateGenre(genreRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("RPG", result.Name);
        Assert.Null(result.ParentGenreId);
        _unitOfWorkMock.Verify(u => u.Genres.AddAsync(It.IsAny<Genre>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    /// <summary>
    /// Test that UpdateGenre updates existing genre.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateGenreShouldUpdateExistingGenre()
    {
        // Arrange
        var genreId = Guid.NewGuid();
        var existingGenre = new Genre
        {
            Id = genreId,
            Name = "Old Name",
            ParentGenreId = null,
        };

        var updateRequest = new GenreMetadataUpdateRequestDto
        {
            Id = genreId,
            Name = "Updated Name",
            ParentGenreId = null,
        };

        _unitOfWorkMock
            .Setup(u => u.Genres.GetByIdAsync(genreId))
            .ReturnsAsync(existingGenre);

        _unitOfWorkMock
            .Setup(u => u.Genres.GetAllAsync())
            .ReturnsAsync(new List<Genre> { existingGenre });

        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _genreService.UpdateGenre(genreId, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(genreId, result.Id);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("Updated Name", existingGenre.Name);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    /// <summary>
    /// Test that DeleteGenreById deletes genre successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DeleteGenreByIdShouldDeleteGenreSuccessfully()
    {
        // Arrange
        var genreId = Guid.NewGuid();
        var genre = new Genre
        {
            Id = genreId,
            Name = "Genre To Delete",
            ParentGenreId = null,
        };

        _unitOfWorkMock
            .Setup(u => u.Genres.GetByIdAsync(genreId))
            .ReturnsAsync(genre);

        _unitOfWorkMock
            .Setup(u => u.Genres.GetAllAsync())
            .ReturnsAsync(new List<Genre> { genre });

        _unitOfWorkMock
            .Setup(u => u.GameGenres.GetByGenreIdAsync(genreId))
            .ReturnsAsync(new List<GameGenre>());

        _unitOfWorkMock
            .Setup(u => u.Genres.DeleteAsync(genreId))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _genreService.DeleteGenreById(genreId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(genreId, result.Id);
        Assert.Equal("Genre To Delete", result.Name);
        _unitOfWorkMock.Verify(u => u.Genres.DeleteAsync(genreId), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    /// <summary>
    /// Test that GetSubGenresAsync returns sub-genres for parent genre.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetSubGenresAsyncShouldReturnSubGenres()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var parentGenre = new Genre
        {
            Id = parentId,
            Name = "Parent Genre",
            ParentGenreId = null,
        };

        var subGenres = new List<Genre>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Sub Genre 1",
                ParentGenreId = parentId,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Sub Genre 2",
                ParentGenreId = parentId,
            },
        };

        var allGenres = new List<Genre> { parentGenre };
        allGenres.AddRange(subGenres);

        _unitOfWorkMock
            .Setup(u => u.Genres.GetByIdAsync(parentId))
            .ReturnsAsync(parentGenre);

        _unitOfWorkMock
            .Setup(u => u.Genres.GetAllAsync())
            .ReturnsAsync(allGenres);

        // Act
        var result = await _genreService.GetSubGenresAsync(parentId);
        var resultList = result.ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, resultList.Count);
        Assert.All(resultList, g => Assert.Equal(parentId, g.ParentGenreId));
        Assert.Contains(resultList, g => g.Name == "Sub Genre 1");
        Assert.Contains(resultList, g => g.Name == "Sub Genre 2");
    }
}