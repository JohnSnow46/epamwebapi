using Gamestore.Entities.Community;

namespace Gamestore.Data.Interfaces;
public interface ICommentRepository : IRepository<Comment>
{
    Task<IEnumerable<Comment>> GetCommentsByGameKeyAsync(string gameKey);

    Task<Comment?> GetCommentByIdAsync(Guid id);

    Task<IEnumerable<Comment>> GetRootCommentsByGameIdAsync(Guid gameId);
}