using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Gamestore.Services.Dto.PlatformsDto;
using Gamestore.Services.Services.Business;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.Tests.Services.Tests;

/// <summary>
/// Unit tests for PlatformService.
/// </summary>
public class PlatformServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<PlatformService>> _loggerMock;
    private readonly PlatformService _platformService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformServiceTests"/> class.
    /// </summary>
    public PlatformServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<PlatformService>>();
        _platformService = new PlatformService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Test that CreatePlatform creates new platform successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CreatePlatformShouldCreateNewPlatform()
    {
        // Arrange
        var platformRequest = new PlatformMetadataCreateRequestDto
        {
            Platform = new PlatformCreateRequestDto
            {
                Type = "PlayStation 5",
            },
        };

        _unitOfWorkMock
            .Setup(u => u.Platforms.GetAllAsync())
            .ReturnsAsync(new List<Platform>());

        _unitOfWorkMock
            .Setup(u => u.Platforms.AddAsync(It.IsAny<Platform>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _platformService.CreatePlatform(platformRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("PlayStation 5", result.Type);
        _unitOfWorkMock.Verify(u => u.Platforms.AddAsync(It.IsAny<Platform>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    /// <summary>
    /// Test that UpdatePlatform updates existing platform successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task UpdatePlatformShouldUpdateExistingPlatform()
    {
        // Arrange
        var platformId = Guid.NewGuid();
        var existingPlatform = new Platform
        {
            Id = platformId,
            Type = "Old Type",
        };

        var updateRequest = new PlatformMetadataUpdateRequestDto
        {
            Id = platformId,
            Type = "Updated Type",
        };

        _unitOfWorkMock
            .Setup(u => u.Platforms.GetByIdAsync(platformId))
            .ReturnsAsync(existingPlatform);

        _unitOfWorkMock
            .Setup(u => u.Platforms.GetAllAsync())
            .ReturnsAsync(new List<Platform> { existingPlatform });

        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _platformService.UpdatePlatform(platformId, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(platformId, result.Id);
        Assert.Equal("Updated Type", result.Type);
        Assert.Equal("Updated Type", existingPlatform.Type);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    /// <summary>
    /// Test that GetAllPlatformsAsync returns all platforms.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetAllPlatformsAsyncShouldReturnAllPlatforms()
    {
        // Arrange
        var platforms = new List<Platform>
        {
            new() { Id = Guid.NewGuid(), Type = "PC" },
            new() { Id = Guid.NewGuid(), Type = "PlayStation" },
            new() { Id = Guid.NewGuid(), Type = "Xbox" },
        };

        _unitOfWorkMock
            .Setup(u => u.Platforms.GetAllAsync())
            .ReturnsAsync(platforms);

        // Act
        var result = await _platformService.GetAllPlatformsAsync();
        var resultList = result.ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, resultList.Count);
        Assert.Contains(resultList, p => p.Type == "PC");
        Assert.Contains(resultList, p => p.Type == "PlayStation");
        Assert.Contains(resultList, p => p.Type == "Xbox");
    }

    /// <summary>
    /// Test that GetPlatformById returns platform when it exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetPlatformByIdShouldReturnPlatformWhenExists()
    {
        // Arrange
        var platformId = Guid.NewGuid();
        var platform = new Platform
        {
            Id = platformId,
            Type = "Nintendo Switch",
        };

        _unitOfWorkMock
            .Setup(u => u.Platforms.GetByIdAsync(platformId))
            .ReturnsAsync(platform);

        // Act
        var result = await _platformService.GetPlatformById(platformId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(platformId, result.Id);
        Assert.Equal("Nintendo Switch", result.Type);
    }

    /// <summary>
    /// Test that DeletePlatform deletes platform successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DeletePlatformShouldDeletePlatformSuccessfully()
    {
        // Arrange
        var platformId = Guid.NewGuid();
        var platform = new Platform
        {
            Id = platformId,
            Type = "Platform To Delete",
        };

        _unitOfWorkMock
            .Setup(u => u.Platforms.GetByIdAsync(platformId))
            .ReturnsAsync(platform);

        _unitOfWorkMock
            .Setup(u => u.GamePlatforms.GetByPlatformIdAsync(platformId))
            .ReturnsAsync(new List<GamePlatform>());

        _unitOfWorkMock
            .Setup(u => u.Platforms.DeleteAsync(platformId))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _platformService.DeletePlatformById(platformId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(platformId, result.Id);
        Assert.Equal("Platform To Delete", result.Type);
        _unitOfWorkMock.Verify(u => u.Platforms.DeleteAsync(platformId), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    /// <summary>
    /// Test that GetPlatformsByGameKeyAsync returns platforms for game.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetPlatformsByGameKeyAsyncShouldReturnPlatformsForGame()
    {
        // Arrange
        var gameKey = "test-game";
        var gameId = Guid.NewGuid();
        var game = new Game { Id = gameId, Key = gameKey };

        var platform1Id = Guid.NewGuid();
        var platform2Id = Guid.NewGuid();

        var gamePlatforms = new List<GamePlatform>
        {
            new() { GameId = gameId, PlatformId = platform1Id },
            new() { GameId = gameId, PlatformId = platform2Id },
        };

        var platforms = new List<Platform>
        {
            new() { Id = platform1Id, Type = "PC" },
            new() { Id = platform2Id, Type = "PlayStation" },
        };

        _unitOfWorkMock
            .Setup(u => u.Games.GetKeyAsync(gameKey))
            .ReturnsAsync(game);

        _unitOfWorkMock
            .Setup(u => u.GamePlatforms.GetByGameIdAsync(gameId))
            .ReturnsAsync(gamePlatforms);

        _unitOfWorkMock
            .Setup(u => u.GamePlatforms.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(platforms);

        // Act
        var result = await _platformService.GetPlatformsByGameKeyAsync(gameKey);
        var resultList = result.ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, p => p.Type == "PC");
        Assert.Contains(resultList, p => p.Type == "PlayStation");
    }
}