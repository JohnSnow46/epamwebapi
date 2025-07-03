using System.ComponentModel.DataAnnotations;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Gamestore.Services.Dto.PublishersDto;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.Business;

/// <summary>
/// Service for managing publishers in the Gamestore.
/// </summary>
public class PublisherService(IUnitOfWork unitOfWork, ILogger<PublisherService> logger) : IPublisherService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<PublisherService> _logger = logger;

    #region Public Publisher methods

    /// <summary>
    /// Gets all publishers from the database.
    /// </summary>
    public async Task<IEnumerable<Publisher>> GetAllPublishersAsync()
    {
        _logger.LogInformation("Starting get all publishers operation");

        var publishers = await _unitOfWork.Publishers.GetAllAsync();
        _logger.LogInformation("Retrieved {Count} publishers from database", publishers.Count());
        return publishers;
    }

    /// <summary>
    /// Gets a publisher by its ID.
    /// </summary>
    public async Task<Publisher?> GetPublisherByIdAsync(Guid id)
    {
        _logger.LogInformation("Starting get publisher operation by ID: {PublisherId}", id);
        ValidateGuid(id, "Publisher ID");

        var publisher = await _unitOfWork.Publishers.GetByIdAsync(id);
        LogPublisherRetrievalResult(publisher, id);
        return publisher;
    }

    /// <summary>
    /// Gets a publisher by its company name.
    /// </summary>
    public async Task<Publisher?> GetPublisherByCompanyNameAsync(string companyName)
    {
        _logger.LogInformation("Starting get publisher operation by company name: {CompanyName}", companyName);
        ValidateString(companyName, "Company name");

        var publisher = await _unitOfWork.Publishers.GetByCompanyNameAsync(companyName);
        LogPublisherNameRetrievalResult(publisher, companyName);
        return publisher;
    }

    /// <summary>
    /// Adds a new publisher to the database.
    /// </summary>
    public async Task<PublisherCreateRequestDto> AddPublisherAsync(PublisherCreateRequestDto publisher)
    {
        _logger.LogInformation("Starting add publisher operation");

        ValidateObject(publisher, "Publisher DTO");
        ValidateObject(publisher.Publisher, "Publisher data");

        var publisherEntity = CreatePublisherEntityFromDto(publisher.Publisher);

        await _unitOfWork.Publishers.AddAsync(publisherEntity);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Successfully added publisher with ID: {PublisherId}", publisherEntity.Id);
        return publisher;
    }

    /// <summary>
    /// Creates a new publisher.
    /// </summary>
    public async Task<Publisher> CreatePublisherAsync(Publisher publisher)
    {
        _logger.LogInformation("Starting create publisher operation");

        ValidatePublisherEntity(publisher);
        return await CreateNewPublisher(publisher);
    }

    /// <summary>
    /// Updates an existing publisher.
    /// </summary>
    public async Task<Publisher> UpdatePublisherAsync(Guid id, PublisherUpdateRequestDto publisherUpdateDto)
    {
        _logger.LogInformation("Starting update publisher operation for publisher {PublisherId}", id);

        ValidateId(id);

        var existingPublisher = await _unitOfWork.Publishers.GetByIdAsync(id);

        existingPublisher.CompanyName = publisherUpdateDto.CompanyName;
        existingPublisher.HomePage = publisherUpdateDto.HomePage ?? existingPublisher.HomePage;
        existingPublisher.Description = publisherUpdateDto.Description ?? existingPublisher.Description;

        ValidatePublisherEntity(existingPublisher);

        return await UpdateExistingPublisher(existingPublisher);
    }

    /// <summary>
    /// Deletes a publisher by its ID.
    /// </summary>
    public async Task<PublisherMetadataCreateRequestDto> DeletePublisherAsync(Guid id)
    {

        _logger.LogInformation("Starting delete publisher operation for ID: {PublisherId}", id);
        ValidateGuid(id, "Publisher ID");

        var publisher = await GetRequiredPublisherById(id);
        var publisherDto = MapToPublisherDto(publisher);

        await _unitOfWork.Publishers.DeleteAsync(id);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Publisher with ID: {PublisherId} deleted successfully", id);
        return publisherDto;
    }

    /// <summary>
    /// Gets all games for a publisher by company name.
    /// </summary>
    public async Task<IEnumerable<Game>> GetGamesByPublisherNameAsync(string publisherName)
    {
        _logger.LogInformation("Starting get games by publisher operation: {PublisherName}", publisherName);
        ValidateString(publisherName, "Publisher name");

        var publisher = await GetPublisherByNameOrThrow(publisherName);
        var games = await GetGamesForPublisher(publisher.Id);

        _logger.LogInformation("Found {Count} games for publisher: {PublisherName}", games.Count(), publisherName);
        return games;
    }

    /// <summary>
    /// Gets a publisher by a game's key.
    /// </summary>
    public async Task<Publisher> GetPublisherByGameKey(string gameKey)
    {
        _logger.LogInformation("Starting get publisher by game key operation: {GameKey}", gameKey);
        ValidateString(gameKey, "Game key");

        var game = await GetGameByKeyOrThrow(gameKey);
        EnsureGameHasPublisher(game);

        return await GetRequiredPublisherById(game.PublisherId!.Value);
    }

    #endregion

    #region Private Helper Methods

    private void ValidateObject(object? obj, string paramName)
    {
        if (obj == null)
        {
            _logger.LogWarning("{ParamName} is null", paramName);
            throw new ArgumentNullException(paramName.ToLower(System.Globalization.CultureInfo.CurrentCulture).Replace(" ", string.Empty), $"{paramName} cannot be null");
        }
    }

    private void ValidateGuid(Guid id, string paramName)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Provided {ParamName} is empty", paramName);
            throw new ArgumentException($"{paramName} cannot be empty", paramName.ToLower(System.Globalization.CultureInfo.CurrentCulture).Replace(" ", string.Empty));
        }
    }

    private void ValidateString(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _logger.LogWarning("Provided {ParamName} is null or empty", paramName);
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName.ToLower(System.Globalization.CultureInfo.CurrentCulture).Replace(" ", string.Empty));
        }
    }

    private static void ValidateId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Publisher ID cannot be empty", nameof(id));
        }
    }

    private void ValidatePublisherEntity(Publisher? publisher)
    {
        ValidateObject(publisher, "Publisher");

        if (string.IsNullOrWhiteSpace(publisher!.CompanyName))
        {
            _logger.LogWarning("Publisher company name is empty");
            throw new ValidationException("Publisher company name cannot be empty");
        }
    }

    private void LogPublisherRetrievalResult(Publisher? publisher, Guid id)
    {
        if (publisher != null)
        {
            _logger.LogInformation("Publisher found with ID: {PublisherId}", id);
        }
        else
        {
            _logger.LogWarning("Publisher not found with ID: {PublisherId}", id);
        }
    }

    private void LogPublisherNameRetrievalResult(Publisher? publisher, string companyName)
    {
        if (publisher != null)
        {
            _logger.LogInformation("Publisher found with company name: {CompanyName}", companyName);
        }
        else
        {
            _logger.LogWarning("Publisher not found with company name: {CompanyName}", companyName);
        }
    }

    private async Task<Publisher> GetRequiredPublisherById(Guid id)
    {
        var publisher = await _unitOfWork.Publishers.GetByIdAsync(id);

        if (publisher == null)
        {
            _logger.LogWarning("Publisher with ID: {PublisherId} not found", id);
            throw new KeyNotFoundException($"Publisher with ID '{id}' not found");
        }

        return publisher;
    }

    private async Task<Publisher> GetPublisherByNameOrThrow(string publisherName)
    {
        var publisher = await _unitOfWork.Publishers.GetByCompanyNameAsync(publisherName);

        if (publisher == null)
        {
            _logger.LogWarning("Publisher with name {PublisherName} not found", publisherName);
            throw new KeyNotFoundException($"Publisher with name '{publisherName}' not found");
        }

        return publisher;
    }

    private async Task<IEnumerable<Game>> GetGamesForPublisher(Guid publisherId)
    {
        var games = await _unitOfWork.Publishers.GetGamesByPublisherIdAsync(publisherId);

        return games.Select(game => new Game
        {
            Id = game.Id,
            Name = game.Name,
            Key = game.Key,
        });
    }

    private async Task<Game> GetGameByKeyOrThrow(string gameKey)
    {
        var game = await _unitOfWork.Games.GetKeyAsync(gameKey);

        if (game == null)
        {
            _logger.LogWarning("Game with key: {GameKey} not found", gameKey);
            throw new KeyNotFoundException($"Game with key '{gameKey}' not found");
        }

        return game;
    }

    private void EnsureGameHasPublisher(Game game)
    {
        if (game.PublisherId == null)
        {
            _logger.LogWarning("Game with key: {GameKey} has no publisher assigned", game.Key);
            throw new KeyNotFoundException($"Game with key '{game.Key}' has no publisher assigned");
        }
    }

    private async Task<Publisher> CreateNewPublisher(Publisher publisher)
    {
        publisher = EnsurePublisherHasId(publisher);

        _logger.LogInformation("Creating new publisher with ID: {PublisherId}", publisher.Id);

        await _unitOfWork.Publishers.AddAsync(publisher);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Publisher created successfully with ID: {PublisherId}", publisher.Id);

        return publisher;
    }

    private async Task<Publisher> UpdateExistingPublisher(Publisher publisher)
    {
        _logger.LogInformation("Updating existing publisher with ID: {PublisherId}", publisher.Id);

        await _unitOfWork.Publishers.UpdateAsync(publisher);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Publisher updated successfully with ID: {PublisherId}", publisher.Id);

        return publisher;
    }

    private static Publisher EnsurePublisherHasId(Publisher publisher)
    {
        if (publisher.Id == Guid.Empty)
        {
            publisher.Id = Guid.NewGuid();
        }

        return publisher;
    }

    private static Publisher CreatePublisherEntityFromDto(PublisherMetadataCreateRequestDto publisherDto)
    {
        return new Publisher
        {
            Id = Guid.NewGuid(),
            CompanyName = publisherDto.CompanyName ?? string.Empty,
            Description = publisherDto.Description ?? string.Empty,
            HomePage = publisherDto.HomePage ?? string.Empty,
        };
    }

    private static PublisherMetadataCreateRequestDto MapToPublisherDto(Publisher publisher)
    {
        return new PublisherMetadataCreateRequestDto
        {
            Id = publisher.Id,
            CompanyName = publisher.CompanyName,
            HomePage = publisher.HomePage,
            Description = publisher.Description,
        };
    }

    #endregion
}