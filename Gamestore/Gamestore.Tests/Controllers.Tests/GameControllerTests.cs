using Gamestore.Entities.Business;
using Gamestore.Services.Dto.GamesDto;
using Gamestore.Services.Interfaces;
using Gamestore.WebApi.Controllers.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.Tests.Controllers.Tests;

/// <summary>
/// Unit tests for GameController.
/// </summary>
public class GameControllerTests
{
    private readonly Mock<IGameService> _gameServiceMock;
    private readonly Mock<IPublisherService> _publisherServiceMock;
    private readonly Mock<ILogger<GameController>> _loggerMock;
    private readonly GameController _controller;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameControllerTests"/> class.
    /// </summary>
    public GameControllerTests()
    {
        _gameServiceMock = new Mock<IGameService>();
        _publisherServiceMock = new Mock<IPublisherService>();
        _loggerMock = new Mock<ILogger<GameController>>();
        _controller = new GameController(
            _gameServiceMock.Object,
            _publisherServiceMock.Object,
            _loggerMock.Object);
    }

    /// <summary>
    /// Test that CreateGame returns Ok when game is successfully created.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CreateGameShouldReturnOkWhenGameIsCreated()
    {
        // Arrange
        var gameRequest = new GameMetadataCreateRequestDto
        {
            Game = new GameCreateRequestDto
            {
                Name = "Test Game",
                Key = "test-game",
                Description = "Test Description",
                Price = 59.99,
                UnitInStock = 100,
                Discount = 0,
            },
            Publisher = Guid.NewGuid(),
            Genres = new List<Guid> { Guid.NewGuid() },
            Platforms = new List<Guid> { Guid.NewGuid() },
        };

        _gameServiceMock
            .Setup(s => s.AddGameAsync(It.IsAny<GameMetadataCreateRequestDto>()))
            .ReturnsAsync(gameRequest);

        // Act
        var result = await _controller.CreateGame(gameRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _gameServiceMock.Verify(s => s.AddGameAsync(It.IsAny<GameMetadataCreateRequestDto>()), Times.Once);
    }

    /// <summary>
    /// Test that CreateGame returns InternalServerError when service returns null.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CreateGameShouldReturnInternalServerErrorWhenServiceReturnsNull()
    {
        // Arrange
        var gameRequest = new GameMetadataCreateRequestDto
        {
            Game = new GameCreateRequestDto
            {
                Name = "Test Game",
                Key = "test-game",
                Description = "Test Description",
                Price = 59.99,
                UnitInStock = 100,
                Discount = 0,
            },
            Publisher = Guid.NewGuid(),
        };

        _gameServiceMock
            .Setup(s => s.AddGameAsync(It.IsAny<GameMetadataCreateRequestDto>()))
            .ReturnsAsync((GameMetadataCreateRequestDto?)null);

        // Act
        var result = await _controller.CreateGame(gameRequest);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    /// <summary>
    /// Test that GetGameByKey returns Ok when game exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetGameByKeyShouldReturnOkWhenGameExists()
    {
        // Arrange
        var key = "test-game";
        var gameDto = new GameUpdateRequestDto
        {
            Key = key,
            Name = "Test Game",
            Description = "Test Description",
            Price = 59.99,
            UnitInStock = 100,
            Discontinued = 0,
        };

        _gameServiceMock
            .Setup(s => s.GetGameByKey(key))
            .ReturnsAsync(gameDto);

        // Act
        var result = await _controller.GetGameByKey(key);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGame = Assert.IsType<GameUpdateRequestDto>(okResult.Value);
        Assert.Equal(key, returnedGame.Key);
        Assert.Equal("Test Game", returnedGame.Name);
    }

    /// <summary>
    /// Test that GetGameByKey returns NotFound when game does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetGameByKeyShouldReturnNotFoundWhenGameDoesNotExist()
    {
        // Arrange
        var key = "nonexistent-game";

        _gameServiceMock
            .Setup(s => s.GetGameByKey(key))
            .ReturnsAsync((GameUpdateRequestDto?)null);

        // Act
        var result = await _controller.GetGameByKey(key);

        // Assert
        var notFoundResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    /// <summary>
    /// Test that GetGameById returns Ok when game exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetGameByIdShouldReturnOkWhenGameExists()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var gameDto = new GameCreateRequestDto
        {
            Name = "Test Game",
            Key = "test-game",
            Description = "Test Description",
            Price = 59.99,
            UnitInStock = 100,
            Discount = 0,
        };

        _gameServiceMock
            .Setup(s => s.GetGameById(gameId))
            .ReturnsAsync(gameDto);

        // Act
        var result = await _controller.GetGameById(gameId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGame = Assert.IsType<GameCreateRequestDto>(okResult.Value);
        Assert.Equal("Test Game", returnedGame.Name);
    }

    /// <summary>
    /// Test that GetAllGames returns Ok with games list.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetAllGamesShouldReturnOkWhenGamesExist()
    {
        // Arrange
        var games = new List<GameCreateRequestDto>
        {
            new()
            {
                Name = "Game 1",
                Key = "game-1",
                Description = "Description 1",
                Price = 29.99,
                UnitInStock = 50,
                Discount = 0,
            },
            new()
            {
                Name = "Game 2",
                Key = "game-2",
                Description = "Description 2",
                Price = 39.99,
                UnitInStock = 75,
                Discount = 10,
            },
        };

        _gameServiceMock
            .Setup(s => s.GetAllGames())
            .ReturnsAsync(games);

        // Act
        var result = await _controller.GetAllGames();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGames = Assert.IsAssignableFrom<IEnumerable<GameCreateRequestDto>>(okResult.Value);
        Assert.Equal(2, returnedGames.Count());
    }

    /// <summary>
    /// Test that DeleteGame returns Ok when game is deleted.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DeleteGameShouldReturnOkWhenGameIsDeleted()
    {
        // Arrange
        var key = "game-to-delete";
        var deletedGame = new Game
        {
            Id = Guid.NewGuid(),
            Key = key,
            Name = "Deleted Game",
            Description = "This game was deleted",
        };

        _gameServiceMock
            .Setup(s => s.DeleteGameAsync(key))
            .ReturnsAsync(deletedGame);

        // Act
        var result = await _controller.DeleteGameByKey(key);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _gameServiceMock.Verify(s => s.DeleteGameAsync(key), Times.Once);
    }

    /// <summary>
    /// Test that GetGamesByGenre returns Ok when games exist for genre.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetGamesByGenreShouldReturnOkWhenGamesExist()
    {
        // Arrange
        var genreId = Guid.NewGuid();
        var games = new List<GameCreateRequestDto>
        {
            new()
            {
                Name = "Action Game",
                Key = "action-game",
                Description = "An action game",
                Price = 49.99,
                UnitInStock = 100,
                Discount = 0,
            },
        };

        _gameServiceMock
            .Setup(s => s.GetGamesByGenreAsync(genreId))
            .ReturnsAsync(games);

        // Act
        var result = await _controller.GetGameById(genreId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGames = Assert.IsAssignableFrom<IEnumerable<GameCreateRequestDto>>(okResult.Value);
        Assert.Single(returnedGames);
    }
}