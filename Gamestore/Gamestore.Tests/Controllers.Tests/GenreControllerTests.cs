using System.Security.Claims;
using Gamestore.Services.Dto.GenresDto;
using Gamestore.Services.Interfaces;
using Gamestore.WebApi.Controllers.Business;
using Microsoft.AspNetCore.Http;
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

    public GenreControllerTests()
    {
        _gameServiceMock = new Mock<IGameService>();
        _genreServiceMock = new Mock<IGenreService>();
        _loggerMock = new Mock<ILogger<GenreController>>();

        _controller = new GenreController(
            _gameServiceMock.Object,
            _genreServiceMock.Object,
            _loggerMock.Object);

        SetupControllerContext();
    }

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
    }

    [Fact]
    public async Task GetGenreByIdShouldReturnGenreWhenExists()
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
        Assert.Equal("Adventure", returnedGenre.Name);
    }

    [Fact]
    public async Task GetAllGenresShouldReturnGenresList()
    {
        // Arrange
        var genres = new List<GenreUpdateRequestDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Action", ParentGenreId = null },
            new() { Id = Guid.NewGuid(), Name = "RPG", ParentGenreId = null },
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

    [Fact]
    public async Task GetSubGenresShouldReturnSubGenres()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var subGenres = new List<GenreUpdateRequestDto>
        {
            new() { Id = Guid.NewGuid(), Name = "FPS", ParentGenreId = parentId },
            new() { Id = Guid.NewGuid(), Name = "TPS", ParentGenreId = parentId },
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
    }

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
    }

    [Fact]
    public async Task GetGenresByGameKeyShouldReturnGenres()
    {
        // Arrange
        var gameKey = "test-game";
        var genres = new List<GenreUpdateRequestDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Action", ParentGenreId = null },
            new() { Id = Guid.NewGuid(), Name = "Adventure", ParentGenreId = null },
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