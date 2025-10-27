using Gamestore.Entities.Business;
using Gamestore.Services.Dto.PlatformsDto;
using Gamestore.Services.Interfaces;
using Gamestore.WebApi.Controllers.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.Tests.Controllers.Tests;

/// <summary>
/// Unit tests for PlatformController.
/// </summary>
public class PlatformControllerTests
{
    private readonly Mock<IGameService> _gameServiceMock;
    private readonly Mock<IPlatformService> _platformServiceMock;
    private readonly Mock<ILogger<PlatformController>> _loggerMock;
    private readonly PlatformController _controller;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformControllerTests"/> class.
    /// </summary>
    public PlatformControllerTests()
    {
        _gameServiceMock = new Mock<IGameService>();
        _platformServiceMock = new Mock<IPlatformService>();
        _loggerMock = new Mock<ILogger<PlatformController>>();
        _controller = new PlatformController(
            _gameServiceMock.Object,
            _platformServiceMock.Object,
            _loggerMock.Object);
    }

    /// <summary>
    /// Test that CreateOrUpdatePlatform returns Ok when platform is successfully created.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CreateOrUpdatePlatformShouldReturnOkWhenPlatformIsCreated()
    {
        // Arrange
        var platformRequest = new PlatformMetadataCreateRequestDto
        {
            Platform = new PlatformCreateRequestDto
            {
                Type = "PlayStation 5",
            },
        };

        var createdPlatform = new PlatformCreateRequestDto
        {
            Id = Guid.NewGuid(),
            Type = "PlayStation 5",
        };

        _platformServiceMock
            .Setup(s => s.CreatePlatform(It.IsAny<PlatformMetadataCreateRequestDto>()))
            .ReturnsAsync(createdPlatform);

        // Act
        var result = await _controller.CreateOrUpdatePlatform(platformRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlatform = Assert.IsType<PlatformCreateRequestDto>(okResult.Value);
        Assert.Equal("PlayStation 5", returnedPlatform.Type);
        _platformServiceMock.Verify(s => s.CreatePlatform(It.IsAny<PlatformMetadataCreateRequestDto>()), Times.Once);
    }

    /// <summary>
    /// Test that CreateOrUpdatePlatform returns NotFound when platform creation fails.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CreateOrUpdatePlatformShouldReturnNotFoundWhenCreationFails()
    {
        // Arrange
        var platformRequest = new PlatformMetadataCreateRequestDto
        {
            Platform = new PlatformCreateRequestDto
            {
                Type = "Invalid Platform",
            },
        };

        _platformServiceMock
            .Setup(s => s.CreatePlatform(It.IsAny<PlatformMetadataCreateRequestDto>()))
            .ReturnsAsync((PlatformCreateRequestDto?)null);

        // Act
        var result = await _controller.CreateOrUpdatePlatform(platformRequest);

        // Assert
        var notFoundResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    /// <summary>
    /// Test that GetPlatformById returns Ok when platform exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetPlatformByIdShouldReturnOkWhenPlatformExists()
    {
        // Arrange
        var platformId = Guid.NewGuid();
        var platform = new Platform
        {
            Id = platformId,
            Type = "Xbox Series X",
        };

        _platformServiceMock
            .Setup(s => s.GetPlatformById(platformId))
            .ReturnsAsync(platform);

        // Act
        var result = await _controller.GetPlatformById(platformId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlatform = Assert.IsType<Platform>(okResult.Value);
        Assert.Equal(platformId, returnedPlatform.Id);
        Assert.Equal("Xbox Series X", returnedPlatform.Type);
    }

    /// <summary>
    /// Test that GetPlatformById returns NotFound when platform does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetPlatformByIdShouldReturnNotFoundWhenPlatformDoesNotExist()
    {
        // Arrange
        var platformId = Guid.NewGuid();

        _platformServiceMock
            .Setup(s => s.GetPlatformById(platformId))
            .ReturnsAsync((Platform?)null);

        // Act
        var result = await _controller.GetPlatformById(platformId);

        // Assert
        var notFoundResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    /// <summary>
    /// Test that GetAllPlatforms returns Ok with platforms list.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetAllPlatformsShouldReturnOkWhenPlatformsExist()
    {
        // Arrange
        var platforms = new List<Platform>
        {
            new() { Id = Guid.NewGuid(), Type = "PC" },
            new() { Id = Guid.NewGuid(), Type = "Mobile" },
            new() { Id = Guid.NewGuid(), Type = "Nintendo Switch" },
        };

        _platformServiceMock
            .Setup(s => s.GetAllPlatformsAsync())
            .ReturnsAsync(platforms);

        // Act
        var result = await _controller.GetAllPlatforms();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPlatforms = Assert.IsAssignableFrom<IEnumerable<Platform>>(okResult.Value);
        Assert.Equal(3, returnedPlatforms.Count());
    }

    /// <summary>
    /// Test that GetPlatformsByGameKey returns Ok when platforms exist for game.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetPlatformsByGameKeyShouldReturnOkWhenPlatformsExist()
    {
        // Arrange
        var gameKey = "test-game";
        var platforms = new List<Platform>
        {
            new() { Id = Guid.NewGuid(), Type = "PlayStation 5" },
            new() { Id = Guid.NewGuid(), Type = "Xbox Series X" },
        };

        _platformServiceMock
            .Setup(s => s.GetPlatformsByGameKeyAsync(gameKey))
            .ReturnsAsync(platforms);

        // Act
        var result = await _controller.GetPlatformsByGameKey(gameKey);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlatforms = Assert.IsAssignableFrom<IEnumerable<Platform>>(okResult.Value);
        Assert.Equal(2, returnedPlatforms.Count());
    }

    /// <summary>
    /// Test that GetPlatformsByGameKey returns NotFound when no platforms exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetPlatformsByGameKeyShouldReturnNotFoundWhenNoPlatformsExist()
    {
        // Arrange
        var gameKey = "game-without-platforms";

        _platformServiceMock
            .Setup(s => s.GetPlatformsByGameKeyAsync(gameKey))
            .ReturnsAsync(new List<Platform>());

        // Act
        var result = await _controller.GetPlatformsByGameKey(gameKey);

        // Assert
        var notFoundResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    /// <summary>
    /// Test that DeletePlatform returns Ok when platform is deleted.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DeletePlatformShouldReturnOkWhenPlatformIsDeleted()
    {
        // Arrange
        var platformId = Guid.NewGuid();
        var deletedPlatform = new Platform
        {
            Id = platformId,
            Type = "Deleted Platform",
        };

        _platformServiceMock
            .Setup(s => s.DeletePlatformById(platformId))
            .ReturnsAsync(deletedPlatform);

        // Act
        var result = await _controller.DeletePlatformById(platformId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlatform = Assert.IsType<Platform>(okResult.Value);
        Assert.Equal(platformId, returnedPlatform.Id);
        _platformServiceMock.Verify(s => s.DeletePlatformById(platformId), Times.Once);
    }

    /// <summary>
    /// Test that DeletePlatform returns NotFound when platform does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DeletePlatformShouldReturnNotFoundWhenPlatformDoesNotExist()
    {
        // Arrange
        var platformId = Guid.NewGuid();

        _platformServiceMock
            .Setup(s => s.DeletePlatformById(platformId))
            .ReturnsAsync((Platform?)null);

        // Act
        var result = await _controller.DeletePlatformById(platformId);

        // Assert
        var notFoundResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }
}