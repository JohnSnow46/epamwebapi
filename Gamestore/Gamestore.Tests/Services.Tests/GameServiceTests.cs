using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Gamestore.Services.Caching;
using Gamestore.Services.Dto.GamesDto;
using Gamestore.Services.Interfaces;
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
    private readonly Mock<IBlobStorageService> _blobStorageServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;

    public GameServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<GameService>>();
        _blobStorageServiceMock = new Mock<IBlobStorageService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _gameService = new GameService(_unitOfWorkMock.Object, _loggerMock.Object, _cacheServiceMock.Object, _blobStorageServiceMock.Object);
    }

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

        // Cache miss - returns false
        IEnumerable<GameCreateRequestDto> outGames = null;
        _cacheServiceMock
            .Setup(c => c.TryGetValue(
                CacheKeys.AllGames,
                out outGames))
            .Returns(false);

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

    [Fact]
    public async Task GetGameByKeyShouldReturnGameWhenExists()
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

        // Cache miss
        GameUpdateRequestDto outGame = null;
        _cacheServiceMock
            .Setup(c => c.TryGetValue(
                It.IsAny<string>(),
                out outGame))
            .Returns(false);

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

    [Fact]
    public async Task GetGameByKeyShouldReturnNullWhenGameDoesNotExist()
    {
        // Arrange
        var gameKey = "nonexistent-game";

        // Cache miss
        GameUpdateRequestDto outGame = null;
        _cacheServiceMock
            .Setup(c => c.TryGetValue(
                It.IsAny<string>(),
                out outGame))
            .Returns(false);

        _unitOfWorkMock
            .Setup(u => u.Games.GetKeyAsync(gameKey))
            .ReturnsAsync((Game?)null);

        // Act
        var result = await _gameService.GetGameByKey(gameKey);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetGameByIdShouldReturnGameWhenExists()
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
            .Setup(u => u.GameGenres.AddRangeAsync(It.IsAny<IEnumerable<GameGenre>>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.GamePlatforms.AddRangeAsync(It.IsAny<IEnumerable<GamePlatform>>()))
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
            .Setup(u => u.Games.DeleteGameByKey(game))
            .ReturnsAsync(game);

        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .Returns(Task.CompletedTask);

        _blobStorageServiceMock
            .Setup(b => b.GetBlobNameFromGameKey(gameKey))
            .Returns($"games/{gameKey}.jpg");

        _blobStorageServiceMock
            .Setup(b => b.ImageExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _gameService.DeleteGameAsync(gameKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(gameKey, result.Key);
        _unitOfWorkMock.Verify(u => u.Games.DeleteGameByKey(game), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateGameAsyncShouldUpdateExistingGame()
    {
        // Arrange
        var gameKey = "test-game";
        var existingGame = new Game
        {
            Id = Guid.NewGuid(),
            Key = gameKey,
            Name = "Old Name",
            Description = "Old Description",
            Price = 29.99,
            UnitInStock = 50,
            Discontinued = 0,
        };

        var updateRequest = new GameMetadataUpdateRequestDto
        {
            Game = new GameUpdateRequestDto
            {
                Key = gameKey,
                Name = "Updated Name",
                Description = "Updated Description",
                Price = 39.99,
                UnitInStock = 75,
                Discontinued = 5,
            },
            Publisher = Guid.NewGuid(),
            Genres = new List<Guid> { Guid.NewGuid() },
            Platforms = new List<Guid> { Guid.NewGuid() },
        };

        _unitOfWorkMock
            .Setup(u => u.Games.GetKeyAsync(gameKey))
            .ReturnsAsync(existingGame);

        _unitOfWorkMock
            .Setup(u => u.GameGenres.GetByGameIdAsync(existingGame.Id))
            .ReturnsAsync(new List<GameGenre>());

        _unitOfWorkMock
            .Setup(u => u.GamePlatforms.GetByGameIdAsync(existingGame.Id))
            .ReturnsAsync(new List<GamePlatform>());

        _unitOfWorkMock
            .Setup(u => u.GameGenres.AddRangeAsync(It.IsAny<IEnumerable<GameGenre>>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.GamePlatforms.AddRangeAsync(It.IsAny<IEnumerable<GamePlatform>>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .Returns(Task.CompletedTask);

        _blobStorageServiceMock
            .Setup(b => b.GetBlobNameFromGameKey(gameKey))
            .Returns($"games/{gameKey}.jpg");

        _blobStorageServiceMock
            .Setup(b => b.ImageExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _gameService.UpdateGameAsync(gameKey, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal(39.99, result.Price);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task AddGameAsyncShouldGenerateKeyWhenNotProvided()
    {
        // Arrange
        var gameRequest = new GameMetadataCreateRequestDto
        {
            Game = new GameCreateRequestDto
            {
                Name = "Test Game Without Key",
                Key = null,
                Description = "A test game",
                Price = 59.99,
                UnitInStock = 100,
                Discount = 0,
            },
            Publisher = Guid.NewGuid(),
            Genres = new List<Guid>(),
            Platforms = new List<Guid>(),
        };

        _unitOfWorkMock
            .Setup(u => u.Games.GetKeyAsync(It.IsAny<string>()))
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
        Assert.NotNull(result.Game.Key);
        Assert.NotEmpty(result.Game.Key);
        _unitOfWorkMock.Verify(u => u.Games.AddAsync(It.IsAny<Game>()), Times.Once);
    }
}