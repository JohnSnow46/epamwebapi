using Gamestore.Entities.Community;
using Gamestore.Services.Dto.CommentsDto;

namespace Gamestore.Services.Interfaces;
public interface ICommentService
{
    Task<IEnumerable<CommentRequestDto>> GetCommentsByGameKeyAsync(string gameKey);

    Task<CommentRequestDto> AddCommentAsync(string gameKey, CommentMetadataRequestDto commentRequest);

    Task<Comment> DeleteCommentAsync(Guid commentId);

    Task<List<string>> GetBanDurationsAsync();

    Task BanUserAsync(BanCreateRequestDto banRequest);

    Task<bool> IsUserBannedAsync(string userName);
}
