using Gamestore.Entities.Business;
using Gamestore.Services.Dto.PublishersDto;

namespace Gamestore.Services.Interfaces;

public interface IPublisherService
{
    Task<IEnumerable<Publisher>> GetAllPublishersAsync();

    Task<Publisher?> GetPublisherByIdAsync(Guid id);

    Task<Publisher?> GetPublisherByCompanyNameAsync(string companyName);

    Task<PublisherCreateRequestDto> AddPublisherAsync(PublisherCreateRequestDto publisher);

    Task<PublisherMetadataCreateRequestDto> DeletePublisherAsync(Guid id);

    Task<IEnumerable<Game>> GetGamesByPublisherNameAsync(string publisherName);

    Task<Publisher> GetPublisherByGameKey(string gameKey);

    Task<Publisher> CreatePublisherAsync(Publisher publisher);

    Task<Publisher> UpdatePublisherAsync(Guid id, PublisherUpdateRequestDto publisherUpdateDto);
}