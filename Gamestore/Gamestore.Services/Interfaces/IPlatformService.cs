using Gamestore.Entities.Business;
using Gamestore.Services.Dto.PlatformsDto;

namespace Gamestore.Services.Interfaces;

public interface IPlatformService
{
    Task<PlatformMetadataUpdateRequestDto> UpdatePlatform(Guid id, PlatformMetadataUpdateRequestDto platformRequest);

    Task<PlatformCreateRequestDto> CreatePlatform(PlatformMetadataCreateRequestDto platformRequest);

    Task<Platform> DeletePlatformById(Guid id);

    Task<IEnumerable<Platform>> GetAllPlatformsAsync();

    Task<Platform> GetPlatformById(Guid id);

    Task<IEnumerable<Platform>> GetPlatformsByGameKeyAsync(string gameKey);
}
