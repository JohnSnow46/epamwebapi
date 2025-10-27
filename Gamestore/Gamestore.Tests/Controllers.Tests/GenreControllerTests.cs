using Gamestore.Services.Dto.GenresDto;
using Gamestore.Services.Interfaces;
using Gamestore.WebApi.Controllers.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.Tests.Controllers.Tests;

/// <summary>
/// Unit tests for GenreController.
/// </summary>
public class GenreControllerTests
{
    private readonly Mock<IGameService> _gameServiceMock;
    private readonly Mock<IGenreService> _genreServiceMock;
    private readonly Mock<ILogger<GenreController>> _loggerMock;
    private readonly GenreController _controller;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenreControllerTests"/> class.
    /// </summary>
    public GenreControllerTests()
    {
        _gameServiceMock = new Mock<IGameService>();
        _genreServiceMock = new Mock<IGenreService>();
        _loggerMock = new Mock<ILogger<GenreController>>();
        _controller = new GenreController(
            _gameServiceMock.Object,
            _genreServiceMock.Object,
            _loggerMock.Object);
    }

    /// <summary>
    /// Test that CreateGenre returns Ok when genre is successfully created.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CreateGenreShouldReturnOkWhenGenreIsCreated()
    {
        // Arrange
        var genreRequest = new GenreMetadataCreateRequestDto
        {
            Genre = new GenreCreateRequestDto
            {
                Name = "Action",
                ParentGenreId = null,
            },
        };

        var createdGenre = new GenreCreateRequestDto
        {
            Name = "Action",
            ParentGenreId = null,
        };

        _genreServiceMock
            .Setup(s => s.CreateGenre(It.IsAny<GenreCreateRequestDto>()))
            .ReturnsAsync(createdGenre);

        // Act
        var result = await _controller.CreateGenre(genreRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGenre = Assert.IsType<GenreCreateRequestDto>(okResult.Value);
        Assert.Equal("Action", returnedGenre.Name);
        _genreServiceMock.Verify(s => s.CreateGenre(It.IsAny<GenreCreateRequestDto>()), Times.Once);
    }

    /// <summary>
    /// Test that GetGenreById returns Ok when genre exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetGenreByIdShouldReturnOkWhenGenreExists()
    {
        // Arrange
        var genreId = Guid.NewGuid();
        var genre = new GenreUpdateRequestDto
        {
            Id = genreId,
            Name = "Adventure",
            ParentGenreId = null,
        };

        _genreServiceMock
            .Setup(s => s.GetGenreById(genreId))
            .ReturnsAsync(genre);

        // Act
        var result = await _controller.GetGenreById(genreId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGenre = Assert.IsType<GenreUpdateRequestDto>(okResult.Value);
        Assert.Equal(genreId, returnedGenre.Id);
        Assert.Equal("Adventure", returnedGenre.Name);
    }

    /// <summary>
    /// Test that GetGenreById returns NotFound when genre does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetGenreByIdShouldReturnNotFoundWhenGenreDoesNotExist()
    {
        // Arrange
        var genreId = Guid.NewGuid();

        _genreServiceMock
            .Setup(s => s.GetGenreById(genreId))
            .ReturnsAsync((GenreUpdateRequestDto?)null);

        // Act
        var result = await _controller.GetGenreById(genreId);

        // Assert
        var notFoundResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    /// <summary>
    /// Test that GetAllGenres returns Ok with genres list.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetAllGenresShouldReturnOkWhenGenresExist()
    {
        // Arrange
        var genres = new List<GenreUpdateRequestDto>
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
                Name = "RPG",
                ParentGenreId = null,
            },
        };

        _genreServiceMock
            .Setup(s => s.GetAllGenres())
            .ReturnsAsync(genres);

        // Act
        var result = await _controller.GetAllGenres();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGenres = Assert.IsAssignableFrom<IEnumerable<GenreUpdateRequestDto>>(okResult.Value);
        Assert.Equal(2, returnedGenres.Count());
    }

    /// <summary>
    /// Test that GetSubGenres returns Ok when subgenres exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetSubGenresShouldReturnOkWhenSubGenresExist()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var subGenres = new List<GenreUpdateRequestDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "First Person Shooter",
                ParentGenreId = parentId,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Third Person Shooter",
                ParentGenreId = parentId,
            },
        };

        _genreServiceMock
            .Setup(s => s.GetSubGenresAsync(parentId))
            .ReturnsAsync(subGenres);

        // Act
        var result = await _controller.GetGenresByParentId(parentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSubGenres = Assert.IsAssignableFrom<IEnumerable<GenreUpdateRequestDto>>(okResult.Value);
        Assert.Equal(2, returnedSubGenres.Count());
        Assert.All(returnedSubGenres, sg => Assert.Equal(parentId, sg.ParentGenreId));
    }

    /// <summary>
    /// Test that DeleteGenre returns Ok when genre is deleted.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DeleteGenreShouldReturnOkWhenGenreIsDeleted()
    {
        // Arrange
        var genreId = Guid.NewGuid();
        var deletedGenre = new GenreUpdateRequestDto
        {
            Id = genreId,
            Name = "Deleted Genre",
            ParentGenreId = null,
        };

        _genreServiceMock
            .Setup(s => s.DeleteGenreById(genreId))
            .ReturnsAsync(deletedGenre);

        // Act
        var result = await _controller.DeleteGenreById(genreId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGenre = Assert.IsType<GenreUpdateRequestDto>(okResult.Value);
        Assert.Equal(genreId, returnedGenre.Id);
        _genreServiceMock.Verify(s => s.DeleteGenreById(genreId), Times.Once);
    }

    /// <summary>
    /// Test that DeleteGenre returns NotFound when genre does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DeleteGenreShouldReturnNotFoundWhenGenreDoesNotExist()
    {
        // Arrange
        var genreId = Guid.NewGuid();

        _genreServiceMock
            .Setup(s => s.DeleteGenreById(genreId))
            .ReturnsAsync((GenreUpdateRequestDto?)null);

        // Act
        var result = await _controller.DeleteGenreById(genreId);

        // Assert
        var notFoundResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    /// <summary>
    /// Test that GetGenresByGameKey returns Ok when genres exist for game.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetGenresByGameKeyShouldReturnOkWhenGenresExist()
    {
        // Arrange
        var gameKey = "test-game";
        var genres = new List<GenreUpdateRequestDto>
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

        _genreServiceMock
            .Setup(s => s.GetGenresByGameKeyAsync(gameKey))
            .ReturnsAsync(genres);

        // Act
        var result = await _controller.GetGenresByGameKey(gameKey);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGenres = Assert.IsAssignableFrom<IEnumerable<GenreUpdateRequestDto>>(okResult.Value);
        Assert.Equal(2, returnedGenres.Count());
    }

    /// <summary>
    /// Test that GetGenresByGameKey returns NotFound when no genres exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetGenresByGameKeyShouldReturnNotFoundWhenNoGenresExist()
    {
        // Arrange
        var gameKey = "game-without-genres";

        _genreServiceMock
            .Setup(s => s.GetGenresByGameKeyAsync(gameKey))
            .ReturnsAsync(new List<GenreUpdateRequestDto>());

        // Act
        var result = await _controller.GetGenresByGameKey(gameKey);

        // Assert
        var notFoundResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }
}