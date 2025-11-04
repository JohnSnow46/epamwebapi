using System.Security.Claims;
using Gamestore.Services.Dto.GamesDto;
using Gamestore.Services.Interfaces;
using Gamestore.WebApi.Controllers.Business;
using Microsoft.AspNetCore.Http;
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
    private readonly Mock<IGenreService> _genreServiceMock;
    private readonly Mock<IPlatformService> _platformServiceMock;
    private readonly Mock<IPublisherService> _publisherServiceMock;
    private readonly Mock<ILogger<GameController>> _loggerMock;
    private readonly GameController _controller;

    public GameControllerTests()
    {
        _gameServiceMock = new Mock<IGameService>();
        _genreServiceMock = new Mock<IGenreService>();
        _platformServiceMock = new Mock<IPlatformService>();
        _publisherServiceMock = new Mock<IPublisherService>();
        _loggerMock = new Mock<ILogger<GameController>>();

        _controller = new GameController(
            _gameServiceMock.Object,
            _genreServiceMock.Object,
            _platformServiceMock.Object,
            _publisherServiceMock.Object,
            _loggerMock.Object);

        SetupControllerContext();
    }

    [Fact]
    public async Task CreateGameShouldReturnSuccessWhenGameIsCreated()
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
        Assert.IsAssignableFrom<IActionResult>(result);
        _gameServiceMock.Verify(s => s.AddGameAsync(It.IsAny<GameMetadataCreateRequestDto>()), Times.Once);
    }

    [Fact]
    public async Task GetGameByKeyShouldReturnGameWhenExists()
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
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetAllGamesShouldReturnGamesList()
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

    private void SetupControllerContext()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, "test@example.com"),
            new(ClaimTypes.Role, "Admin"),
            new(ClaimTypes.Name, "Test User"),
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal },
        };
    }
}