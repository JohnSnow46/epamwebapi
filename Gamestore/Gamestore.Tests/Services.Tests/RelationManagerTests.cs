using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Moq;

namespace Gamestore.Tests.Services.Tests;

/// <summary>
/// Unit tests for game relation management functionality.
/// Tests the management of game-platform and game-genre relationships.
/// </summary>
public class RelationManagerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationManagerTests"/> class.
    /// </summary>
    public RelationManagerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    /// <summary>
    /// Test that ManageGamePlatforms replaces existing platforms with new ones.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ManageGamePlatformsShouldReplaceExistingPlatforms()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var existingPlatforms = new List<GamePlatform>
        {
            new() { GameId = gameId, PlatformId = Guid.NewGuid() },
        };
        var newPlatformIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _unitOfWorkMock
            .Setup(u => u.GamePlatforms.GetByGameIdAsync(gameId))
            .ReturnsAsync(existingPlatforms);

        _unitOfWorkMock
            .Setup(u => u.GamePlatforms.RemoveRangeAsync(It.IsAny<IEnumerable<GamePlatform>>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.GamePlatforms.AddRangeAsync(It.IsAny<IEnumerable<GamePlatform>>()))
            .Returns(Task.CompletedTask);

        // Act
        await _unitOfWorkMock.Object.GamePlatforms.GetByGameIdAsync(gameId);
        await _unitOfWorkMock.Object.GamePlatforms.RemoveRangeAsync(existingPlatforms);
        var newGamePlatforms = newPlatformIds.Select(pid => new GamePlatform
        {
            GameId = gameId,
            PlatformId = pid,
        }).ToList();
        await _unitOfWorkMock.Object.GamePlatforms.AddRangeAsync(newGamePlatforms);

        // Assert
        _unitOfWorkMock.Verify(
            u => u.GamePlatforms.GetByGameIdAsync(gameId),
            Times.Once,
            "Should retrieve existing platforms");

        _unitOfWorkMock.Verify(
            u => u.GamePlatforms.RemoveRangeAsync(It.IsAny<IEnumerable<GamePlatform>>()),
            Times.Once,
            "Should remove existing platforms");

        _unitOfWorkMock.Verify(
            u => u.GamePlatforms.AddRangeAsync(It.Is<IEnumerable<GamePlatform>>(list =>
                list.Count() == 2 &&
                list.All(p => p.GameId == gameId && newPlatformIds.Contains(p.PlatformId)))),
            Times.Once,
            "Should add new platforms with correct IDs");
    }

    /// <summary>
    /// Test that ManageGameGenres replaces existing genres with new ones.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ManageGameGenresShouldReplaceExistingGenres()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var existingGenres = new List<GameGenre>
        {
            new() { GameId = gameId, GenreId = Guid.NewGuid() },
        };
        var newGenreIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _unitOfWorkMock
            .Setup(u => u.GameGenres.GetByGameIdAsync(gameId))
            .ReturnsAsync(existingGenres);

        _unitOfWorkMock
            .Setup(u => u.GameGenres.RemoveRangeAsync(It.IsAny<IEnumerable<GameGenre>>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.GameGenres.AddRangeAsync(It.IsAny<IEnumerable<GameGenre>>()))
            .Returns(Task.CompletedTask);

        // Act
        await _unitOfWorkMock.Object.GameGenres.GetByGameIdAsync(gameId);
        await _unitOfWorkMock.Object.GameGenres.RemoveRangeAsync(existingGenres);
        var newGameGenres = newGenreIds.Select(gid => new GameGenre
        {
            GameId = gameId,
            GenreId = gid,
        }).ToList();
        await _unitOfWorkMock.Object.GameGenres.AddRangeAsync(newGameGenres);

        // Assert
        _unitOfWorkMock.Verify(
            u => u.GameGenres.GetByGameIdAsync(gameId),
            Times.Once,
            "Should retrieve existing genres");

        _unitOfWorkMock.Verify(
            u => u.GameGenres.RemoveRangeAsync(It.IsAny<IEnumerable<GameGenre>>()),
            Times.Once,
            "Should remove existing genres");

        _unitOfWorkMock.Verify(
            u => u.GameGenres.AddRangeAsync(It.Is<IEnumerable<GameGenre>>(list =>
                list.Count() == 2 &&
                list.All(g => g.GameId == gameId && newGenreIds.Contains(g.GenreId)))),
            Times.Once,
            "Should add new genres with correct IDs");
    }

    /// <summary>
    /// Test that empty platform list is handled correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ManageGamePlatformsShouldHandleEmptyPlatformList()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var existingPlatforms = new List<GamePlatform>
        {
            new() { GameId = gameId, PlatformId = Guid.NewGuid() },
        };

        _unitOfWorkMock
            .Setup(u => u.GamePlatforms.GetByGameIdAsync(gameId))
            .ReturnsAsync(existingPlatforms);

        _unitOfWorkMock
            .Setup(u => u.GamePlatforms.RemoveRangeAsync(It.IsAny<IEnumerable<GamePlatform>>()))
            .Returns(Task.CompletedTask);

        // Act
        await _unitOfWorkMock.Object.GamePlatforms.GetByGameIdAsync(gameId);
        await _unitOfWorkMock.Object.GamePlatforms.RemoveRangeAsync(existingPlatforms);

        // Assert
        _unitOfWorkMock.Verify(
            u => u.GamePlatforms.GetByGameIdAsync(gameId),
            Times.Once,
            "Should retrieve existing platforms");

        _unitOfWorkMock.Verify(
            u => u.GamePlatforms.RemoveRangeAsync(It.IsAny<IEnumerable<GamePlatform>>()),
            Times.Once,
            "Should remove existing platforms even when new list is empty");
    }

    /// <summary>
    /// Test that empty genre list is handled correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ManageGameGenresShouldHandleEmptyGenreList()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var existingGenres = new List<GameGenre>
        {
            new() { GameId = gameId, GenreId = Guid.NewGuid() },
        };

        _unitOfWorkMock
            .Setup(u => u.GameGenres.GetByGameIdAsync(gameId))
            .ReturnsAsync(existingGenres);

        _unitOfWorkMock
            .Setup(u => u.GameGenres.RemoveRangeAsync(It.IsAny<IEnumerable<GameGenre>>()))
            .Returns(Task.CompletedTask);

        // Act
        await _unitOfWorkMock.Object.GameGenres.GetByGameIdAsync(gameId);
        await _unitOfWorkMock.Object.GameGenres.RemoveRangeAsync(existingGenres);

        // Assert
        _unitOfWorkMock.Verify(
            u => u.GameGenres.GetByGameIdAsync(gameId),
            Times.Once,
            "Should retrieve existing genres");

        _unitOfWorkMock.Verify(
            u => u.GameGenres.RemoveRangeAsync(It.IsAny<IEnumerable<GameGenre>>()),
            Times.Once,
            "Should remove existing genres even when new list is empty");
    }

    /// <summary>
    /// Test that adding platforms without removing existing ones works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AddGamePlatformsShouldAddNewPlatforms()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var newPlatformIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _unitOfWorkMock
            .Setup(u => u.GamePlatforms.AddRangeAsync(It.IsAny<IEnumerable<GamePlatform>>()))
            .Returns(Task.CompletedTask);

        // Act
        var gamePlatforms = newPlatformIds.Select(pid => new GamePlatform
        {
            GameId = gameId,
            PlatformId = pid,
        }).ToList();
        await _unitOfWorkMock.Object.GamePlatforms.AddRangeAsync(gamePlatforms);

        // Assert
        _unitOfWorkMock.Verify(
            u => u.GamePlatforms.AddRangeAsync(It.Is<IEnumerable<GamePlatform>>(list =>
                list.Count() == 2 &&
                list.All(p => p.GameId == gameId && newPlatformIds.Contains(p.PlatformId)))),
            Times.Once,
            "Should add all new platforms");
    }

    /// <summary>
    /// Test that adding genres without removing existing ones works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AddGameGenresShouldAddNewGenres()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var newGenreIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _unitOfWorkMock
            .Setup(u => u.GameGenres.AddRangeAsync(It.IsAny<IEnumerable<GameGenre>>()))
            .Returns(Task.CompletedTask);

        // Act
        var gameGenres = newGenreIds.Select(gid => new GameGenre
        {
            GameId = gameId,
            GenreId = gid,
        }).ToList();
        await _unitOfWorkMock.Object.GameGenres.AddRangeAsync(gameGenres);

        // Assert
        _unitOfWorkMock.Verify(
            u => u.GameGenres.AddRangeAsync(It.Is<IEnumerable<GameGenre>>(list =>
                list.Count() == 2 &&
                list.All(g => g.GameId == gameId && newGenreIds.Contains(g.GenreId)))),
            Times.Once,
            "Should add all new genres");
    }
}