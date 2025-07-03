using Gamestore.Entities.Community;

namespace Gamestore.Data.Interfaces;
public interface IBanRepository : IRepository<Ban>
{
    Task<Ban?> GetActiveBanByUserNameAsync(string userName);

    Task<bool> IsUserBannedAsync(string userName);
}
