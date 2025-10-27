using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Gamestore.Services.Dto.GamesDto;
using Gamestore.Services.Services.Business;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.Tests.Services.Tests;

/// <summary>
/// Unit tests for GameService.
/// </summary>
public class GameServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<GameService>> _loggerMock;
    private readonly GameService _gameService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameServiceTests"/> class.
    /// </summary>
    public GameServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<GameService>>();
        _gameService = new GameService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Test that GetAllGames returns all games.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetAllGamesShouldReturnAllGames()
    {
        // Arrange
        var games = new List<Game>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Key = "game-1",
                Name = "Game 1",
                Description = "Description 1",
                Price = 29.99,
                UnitInStock = 100,
                Discontinued = 0,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Key = "game-2",
                Name = "Game 2",
                Description = "Description 2",
                Price = 39.99,
                UnitInStock = 200,
                Discontinued = 10,
            },
        };

        _unitOfWorkMock
            .Setup(u => u.Games.GetAllAsync())
            .ReturnsAsync(games);

        // Act
        var result = await _gameService.GetAllGames();
        var gamesList = result.ToList();

        // Assert
        Assert.Equal(2, gamesList.Count);
        Assert.Contains(gamesList, g => g.Key == "game-1");
        Assert.Contains(gamesList, g => g.Key == "game-2");
    }

    /// <summary>
    /// Test that GetGameByKey returns game when it exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetGameByKeyShouldReturnGameWhenItExists()
    {
        // Arrange
        var gameKey = "test-game";
        var game = new Game
        {
            Id = Guid.NewGuid(),
            Key = gameKey,
            Name = "Test Game",
            Description = "Test Description",
            Price = 49.99,
            UnitInStock = 150,
            Discontinued = 5,
        };

        _unitOfWorkMock
            .Setup(u => u.Games.GetKeyAsync(gameKey))
            .ReturnsAsync(game);

        // Act
        var result = await _gameService.GetGameByKey(gameKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(gameKey, result.Key);
        Assert.Equal("Test Game", result.Name);
        Assert.Equal(49.99, result.Price);
    }

    /// <summary>
    /// Test that GetGameByKey returns null when game does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetGameByKeyShouldReturnNullWhenGameDoesNotExist()
    {
        // Arrange
        var gameKey = "nonexistent-game";

        _unitOfWorkMock
            .Setup(u => u.Games.GetKeyAsync(gameKey))
            .ReturnsAsync((Game?)null);

        // Act
        var result = await _gameService.GetGameByKey(gameKey);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Test that GetGameById returns game when it exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetGameByIdShouldReturnGameWhenItExists()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = new Game
        {
            Id = gameId,
            Key = "test-game",
            Name = "Test Game",
            Description = "Test Description",
            Price = 59.99,
            UnitInStock = 100,
            Discontinued = 0,
        };

        _unitOfWorkMock
            .Setup(u => u.Games.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _gameService.GetGameById(gameId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Game", result.Name);
        Assert.Equal("test-game", result.Key);
        Assert.Equal(59.99, result.Price);
    }

    /// <summary>
    /// Test that GetTotalGamesCount returns correct count.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetTotalGamesCountShouldReturnCorrectCount()
    {
        // Arrange
        var expectedCount = 42;

        _unitOfWorkMock
            .Setup(u => u.Games.CountAsync())
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _gameService.GetTotalGamesCountAsync();

        // Assert
        Assert.Equal(expectedCount, result);
        _unitOfWorkMock.Verify(u => u.Games.CountAsync(), Times.Once);
    }

    /// <summary>
    /// Test that AddGameAsync successfully adds a new game.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AddGameAsyncShouldAddNewGame()
    {
        // Arrange
        var gameRequest = new GameMetadataCreateRequestDto
        {
            Game = new GameCreateRequestDto
            {
                Name = "New Game",
                Key = "new-game",
                Description = "A new game",
                Price = 69.99,
                UnitInStock = 50,
                Discount = 0,
            },
            Publisher = Guid.NewGuid(),
            Genres = new List<Guid> { Guid.NewGuid() },
            Platforms = new List<Guid> { Guid.NewGuid() },
        };

        _unitOfWorkMock
            .Setup(u => u.Games.GetKeyAsync(gameRequest.Game.Key))
            .ReturnsAsync((Game?)null);

        _unitOfWorkMock
            .Setup(u => u.Games.AddAsync(It.IsAny<Game>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _gameService.AddGameAsync(gameRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Game", result.Game.Name);
        _unitOfWorkMock.Verify(u => u.Games.AddAsync(It.IsAny<Game>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    /// <summary>
    /// Test that DeleteGameAsync successfully deletes a game.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DeleteGameAsyncShouldDeleteGame()
    {
        // Arrange
        var gameKey = "game-to-delete";
        var game = new Game
        {
            Id = Guid.NewGuid(),
            Key = gameKey,
            Name = "Game To Delete",
            Description = "This will be deleted",
            Price = 19.99,
            UnitInStock = 10,
            Discontinued = 0,
        };

        _unitOfWorkMock
            .Setup(u => u.Games.GetKeyAsync(gameKey))
            .ReturnsAsync(game);

        _unitOfWorkMock
            .Setup(u => u.Games.DeleteAsync(game.Id))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _gameService.DeleteGameAsync(gameKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(gameKey, result.Key);
        _unitOfWorkMock.Verify(u => u.Games.DeleteAsync(game.Id), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    /// <summary>
    /// Test that GetGamesByGenreAsync returns games for specific genre.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetGamesByGenreAsyncShouldReturnGamesForGenre()
    {
        // Arrange
        var genreId = Guid.NewGuid();
        var games = new List<Game>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Key = "action-game-1",
                Name = "Action Game 1",
                Description = "First action game",
                Price = 59.99,
                UnitInStock = 100,
                Discontinued = 0,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Key = "action-game-2",
                Name = "Action Game 2",
                Description = "Second action game",
                Price = 49.99,
                UnitInStock = 75,
                Discontinued = 10,
            },
        };

        _unitOfWorkMock
            .Setup(u => u.Games.GetByGenreAsync(genreId))
            .ReturnsAsync(games);

        // Act
        var result = await _gameService.GetGamesByGenreAsync(genreId);
        var gamesList = result.ToList();

        // Assert
        Assert.Equal(2, gamesList.Count);
        Assert.Contains(gamesList, g => g.Name == "Action Game 1");
        Assert.Contains(gamesList, g => g.Name == "Action Game 2");
    }

    /// <summary>
    /// Test that GetGamesByPlatformAsync returns games for specific platform.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetGamesByPlatformAsyncShouldReturnGamesForPlatform()
    {
        // Arrange
        var platformId = Guid.NewGuid();
        var games = new List<Game>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Key = "pc-game",
                Name = "PC Game",
                Description = "A PC exclusive",
                Price = 39.99,
                UnitInStock = 200,
                Discontinued = 0,
            },
        };

        _unitOfWorkMock
            .Setup(u => u.Games.GetByPlatformAsync(platformId))
            .ReturnsAsync(games);

        // Act
        var result = await _gameService.GetGamesByPlatformAsync(platformId);
        var gamesList = result.ToList();

        // Assert
        Assert.Single(gamesList);
        Assert.Equal("PC Game", gamesList[0].Name);
    }
}