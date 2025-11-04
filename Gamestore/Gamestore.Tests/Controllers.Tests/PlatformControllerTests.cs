using System.Security.Claims;
using Gamestore.Entities.Business;
using Gamestore.Services.Dto.PlatformsDto;
using Gamestore.Services.Interfaces;
using Gamestore.WebApi.Controllers.Business;
using Microsoft.AspNetCore.Http;
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

    public PlatformControllerTests()
    {
        _gameServiceMock = new Mock<IGameService>();
        _platformServiceMock = new Mock<IPlatformService>();
        _loggerMock = new Mock<ILogger<PlatformController>>();

        _controller = new PlatformController(
            _gameServiceMock.Object,
            _platformServiceMock.Object,
            _loggerMock.Object);

        SetupControllerContext();
    }

    [Fact]
    public async Task CreatePlatformShouldReturnOkWhenPlatformIsCreated()
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
        var result = await _controller.CreatePlatform(platformRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlatform = Assert.IsType<PlatformCreateRequestDto>(okResult.Value);
        Assert.Equal("PlayStation 5", returnedPlatform.Type);
    }

    [Fact]
    public async Task GetPlatformByIdShouldReturnPlatformWhenExists()
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
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetAllPlatformsShouldReturnPlatformsList()
    {
        // Arrange
        var platforms = new List<Platform>
        {
            new() { Id = Guid.NewGuid(), Type = "PC" },
            new() { Id = Guid.NewGuid(), Type = "PlayStation" },
            new() { Id = Guid.NewGuid(), Type = "Xbox" },
        };

        _platformServiceMock
            .Setup(s => s.GetAllPlatformsAsync())
            .ReturnsAsync(platforms);

        // Act
        var result = await _controller.GetAllPlatforms();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlatforms = Assert.IsAssignableFrom<IEnumerable<Platform>>(okResult.Value);
        Assert.Equal(3, returnedPlatforms.Count());
    }

    [Fact]
    public async Task GetPlatformsByGameKeyShouldReturnPlatforms()
    {
        // Arrange
        var gameKey = "test-game";
        var platforms = new List<Platform>
        {
            new() { Id = Guid.NewGuid(), Type = "PC" },
            new() { Id = Guid.NewGuid(), Type = "PlayStation" },
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

    [Fact]
    public async Task DeletePlatformShouldReturnOkWhenPlatformIsDeleted()
    {
        // Arrange
        var platformId = Guid.NewGuid();
        var platform = new Platform
        {
            Id = platformId,
            Type = "Deleted Platform",
        };

        _platformServiceMock
            .Setup(s => s.DeletePlatformById(platformId))
            .ReturnsAsync(platform);

        // Act
        var result = await _controller.DeletePlatformById(platformId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    private void SetupControllerContext()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, "test@example.com"),
            new(ClaimTypes.Role, "Admin"),
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal },
        };
    }
}