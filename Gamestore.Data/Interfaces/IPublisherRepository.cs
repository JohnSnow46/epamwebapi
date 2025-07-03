using Gamestore.Entities.Business;

namespace Gamestore.Data.Interfaces;
public interface IPublisherRepository : IRepository<Publisher>
{
    Task<Publisher?> GetByCompanyNameAsync(string companyName);

    Task<IEnumerable<Game>> GetGamesByPublisherIdAsync(Guid publisherId);
}
