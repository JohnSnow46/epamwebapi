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
    /// Updates an existing publisher and returns DTO.
    /// </summary>
    public async Task<PublisherUpdateRequestDto> UpdatePublisherAsync(Guid id, PublisherUpdateRequestDto publisherUpdateDto)
    {
        _logger.LogInformation("Starting update publisher operation for publisher {PublisherId}", id);

        ValidateId(id);

        var existingPublisher = await _unitOfWork.Publishers.GetByIdAsync(id);

        if (existingPublisher == null)
        {
            _logger.LogWarning("Publisher not found for update: {PublisherId}", id);
            throw new ArgumentException($"Publisher with ID '{id}' not found", nameof(id));
        }

        existingPublisher.CompanyName = publisherUpdateDto.CompanyName;
        existingPublisher.HomePage = publisherUpdateDto.HomePage ?? existingPublisher.HomePage;
        existingPublisher.Description = publisherUpdateDto.Description ?? existingPublisher.Description;

        ValidatePublisherEntity(existingPublisher);

        await _unitOfWork.Publishers.UpdateAsync(existingPublisher);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Publisher updated successfully with ID: {PublisherId}", id);

        return MapToPublisherUpdateDto(existingPublisher);
    }

    /// <summary>
    /// Deletes a publisher by its ID and returns DTO.
    /// </summary>
    public async Task<PublisherUpdateRequestDto> DeletePublisherAsync(Guid id)
    {
        _logger.LogInformation("Starting delete publisher operation for ID: {PublisherId}", id);
        ValidateGuid(id, "Publisher ID");

        var publisher = await GetRequiredPublisherById(id);

        await _unitOfWork.Publishers.DeleteAsync(id);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Publisher with ID: {PublisherId} deleted successfully", id);
        return MapToPublisherUpdateDto(publisher);
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

    private void ValidateObject(object? obj, string paramName)
    {
        if (obj == null)
        {
            _logger.LogWarning("Validation failed: {ParamName} is null", paramName);
            throw new ArgumentNullException(paramName);
        }
    }

    private void ValidateId(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Validation failed: Publisher ID is empty");
            throw new ArgumentException("Publisher ID cannot be empty", nameof(id));
        }
    }

    private void ValidateGuid(Guid id, string paramName)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Provided {ParamName} is empty", paramName);
            throw new ArgumentException($"{paramName} cannot be empty", paramName.ToLowerInvariant());
        }
    }

    private void ValidateString(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _logger.LogWarning("Validation failed: {ParamName} is null or empty", paramName);
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName.ToLowerInvariant());
        }
    }

    private void ValidatePublisherEntity(Publisher publisher)
    {
        if (string.IsNullOrWhiteSpace(publisher.CompanyName))
        {
            _logger.LogWarning("Validation failed: CompanyName is null or empty");
            throw new ValidationException("Publisher company name cannot be empty");
        }
    }

    private async Task<Publisher> GetRequiredPublisherById(Guid id)
    {
        var publisher = await _unitOfWork.Publishers.GetByIdAsync(id);
        if (publisher == null)
        {
            _logger.LogWarning("Publisher not found: {PublisherId}", id);
            throw new ArgumentException($"Publisher with ID '{id}' not found");
        }

        return publisher;
    }

    private async Task<Publisher> GetPublisherByNameOrThrow(string name)
    {
        var publisher = await _unitOfWork.Publishers.GetByCompanyNameAsync(name);
        if (publisher == null)
        {
            _logger.LogWarning("Publisher not found by name: {PublisherName}", name);
            throw new ArgumentException($"Publisher with name '{name}' not found");
        }

        return publisher;
    }

    private async Task<Game> GetGameByKeyOrThrow(string key)
    {
        var game = await _unitOfWork.Games.GetKeyAsync(key);
        if (game == null)
        {
            _logger.LogWarning("Game not found by key: {GameKey}", key);
            throw new ArgumentException($"Game with key '{key}' not found");
        }

        return game;
    }

    private static void EnsureGameHasPublisher(Game game)
    {
        if (!game.PublisherId.HasValue)
        {
            throw new ArgumentException("Game does not have an associated publisher");
        }
    }

    private async Task<Publisher> CreateNewPublisher(Publisher publisher)
    {
        _logger.LogInformation("Creating new publisher with ID: {PublisherId}", publisher.Id);

        await _unitOfWork.Publishers.AddAsync(publisher);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Publisher created successfully with ID: {PublisherId}", publisher.Id);

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

    private static PublisherUpdateRequestDto MapToPublisherUpdateDto(Publisher publisher)
    {
        return new PublisherUpdateRequestDto
        {
            Id = publisher.Id,
            CompanyName = publisher.CompanyName,
            HomePage = publisher.HomePage,
            Description = publisher.Description,
        };
    }

    private async Task<IEnumerable<Game>> GetGamesForPublisher(Guid publisherId)
    {
        var games = await _unitOfWork.Games.GetAllAsync();
        return games.Where(g => g.PublisherId == publisherId);
    }

    private void LogPublisherRetrievalResult(Publisher? publisher, Guid id)
    {
        if (publisher == null)
        {
            _logger.LogWarning("Publisher not found by ID: {PublisherId}", id);
        }
        else
        {
            _logger.LogInformation("Successfully retrieved publisher with ID: {PublisherId}", id);
        }
    }

    private void LogPublisherNameRetrievalResult(Publisher? publisher, string name)
    {
        if (publisher == null)
        {
            _logger.LogWarning("Publisher not found by name: {PublisherName}", name);
        }
        else
        {
            _logger.LogInformation("Successfully retrieved publisher with name: {PublisherName}", name);
        }
    }
}