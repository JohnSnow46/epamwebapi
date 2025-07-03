using System.ComponentModel.DataAnnotations;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Gamestore.Entities.Community;
using Gamestore.Services.Dto.CommentsDto;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.Community;
public class CommentService(IUnitOfWork unitOfWork, ILogger<CommentService> logger) : ICommentService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<CommentService> _logger = logger;
    private const string DeletedCommentText = "A comment/quote was deleted";

    public async Task<IEnumerable<CommentRequestDto>> GetCommentsByGameKeyAsync(string gameKey)
    {
        _logger.LogInformation("Getting comments for game with key: {GameKey}", gameKey);

        var comments = await _unitOfWork.Comments.GetCommentsByGameKeyAsync(gameKey);
        var commentDtos = MapCommentsToDto(comments.ToList());

        _logger.LogInformation("Successfully retrieved comments for game with key: {GameKey}", gameKey);
        return commentDtos;
    }

    public async Task<CommentRequestDto> AddCommentAsync(string gameKey, CommentMetadataRequestDto commentRequest)
    {

        _logger.LogInformation("Adding comment for game with key: {GameKey}", gameKey);

        ValidateCommentRequest(commentRequest);
        await ValidateUserNotBanned(commentRequest.Comment.Name);

        var game = await GetGameByKeyOrThrow(gameKey);

        Guid? parentId = null;
        if (!string.IsNullOrEmpty(commentRequest.ParentId))
        {
            if (Guid.TryParse(commentRequest.ParentId, out Guid parsedGuid))
            {
                parentId = parsedGuid;
            }
            else
            {
                _logger.LogWarning("Invalid parentId format: {ParentId}", commentRequest.ParentId);
                throw new ValidationException("Invalid parent comment ID format");
            }
        }

        var comment = await CreateCommentEntity(game.Id, commentRequest, parentId);

        await _unitOfWork.Comments.AddAsync(comment);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Successfully added comment with ID: {CommentId}", comment.Id);
        return MapCommentToDto(comment);
    }

    public async Task<Comment> DeleteCommentAsync(Guid commentId)
    {
        _logger.LogInformation("Deleting comment with ID: {CommentId}", commentId);

        var comment = await GetCommentByIdOrThrow(commentId);

        comment.IsDeleted = true;
        comment.Body = DeletedCommentText;

        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Successfully marked comment as deleted with ID: {CommentId}", commentId);
        return comment;
    }

    public async Task<List<string>> GetBanDurationsAsync()
    {
        return await Task.FromResult(new List<string>
        {
            "1 hour",
            "1 day",
            "1 week",
            "1 month",
            "permanent"
        });
    }

    public async Task BanUserAsync(BanCreateRequestDto banRequest)
    {
        _logger.LogInformation("Banning user: {UserName} for duration: {Duration}", banRequest?.User, banRequest?.Duration);

        ValidateBanRequest(banRequest);

        var ban = new Ban
        {
            Id = Guid.NewGuid(),
            UserName = banRequest.User,
            BanStart = DateTime.UtcNow,
            IsPermanent = banRequest.Duration == "permanent",
            BanEnd = CalculateBanEndTime(banRequest.Duration)
        };

        await _unitOfWork.Bans.AddAsync(ban);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Successfully banned user: {UserName}", banRequest.User);
    }

    public async Task<bool> IsUserBannedAsync(string userName)
    {
        return await _unitOfWork.Bans.IsUserBannedAsync(userName);
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

    private async Task<Comment> GetCommentByIdOrThrow(Guid commentId)
    {
        var comment = await _unitOfWork.Comments.GetCommentByIdAsync(commentId);

        if (comment == null)
        {
            _logger.LogWarning("Comment with ID: {CommentId} not found", commentId);
            throw new KeyNotFoundException($"Comment with ID '{commentId}' not found");
        }

        return comment;
    }

    private static void ValidateCommentRequest(CommentMetadataRequestDto commentRequest)
    {
        ArgumentNullException.ThrowIfNull(commentRequest);

        if (string.IsNullOrWhiteSpace(commentRequest.Comment.Name))
        {
            throw new ValidationException("Comment name is required");
        }

        if (string.IsNullOrWhiteSpace(commentRequest.Comment.Body))
        {
            throw new ValidationException("Comment body is required");
        }
    }

    private static void ValidateBanRequest(BanCreateRequestDto? banRequest)
    {
        ArgumentNullException.ThrowIfNull(banRequest);

        if (string.IsNullOrWhiteSpace(banRequest.User))
        {
            throw new ValidationException("User name is required");
        }

        if (string.IsNullOrWhiteSpace(banRequest.Duration))
        {
            throw new ValidationException("Ban duration is required");
        }

        var validDurations = new[] { "1 hour", "1 day", "1 week", "1 month", "permanent" };
        if (!validDurations.Contains(banRequest.Duration))
        {
            throw new ValidationException("Invalid ban duration");
        }
    }

    private async Task ValidateUserNotBanned(string userName)
    {
        if (await IsUserBannedAsync(userName))
        {
            throw new ValidationException($"User '{userName}' is banned and cannot add comments");
        }
    }

    private async Task<Comment> CreateCommentEntity(Guid gameId, CommentMetadataRequestDto commentRequest, Guid? parentId)
    {
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Name = commentRequest.Comment.Name,
            Body = commentRequest.Comment.Body,
            GameId = gameId,
            CreatedAt = DateTime.UtcNow,
            ParentCommentId = parentId
        };

        if (parentId.HasValue)
        {
            var parentComment = await GetCommentByIdOrThrow(parentId.Value);

            if (!string.IsNullOrWhiteSpace(commentRequest.Action))
            {
                switch (commentRequest.Action.ToLowerInvariant())
                {
                    case "reply":
                        comment.Body = $"[{parentComment.Name}], {comment.Body}";
                        break;
                    case "quote":
                        var quotedBody = parentComment.IsDeleted ? DeletedCommentText : parentComment.Body;
                        comment.Body = $"[{quotedBody}], {comment.Body}";
                        break;
                    default:
                        break;
                }
            }
        }

        return comment;
    }

    private static DateTime? CalculateBanEndTime(string duration)
    {
        if (duration == "permanent")
        {
            return null;
        }

        var now = DateTime.UtcNow;

        return duration switch
        {
            "1 hour" => now.AddHours(1),
            "1 day" => now.AddDays(1),
            "1 week" => now.AddDays(7),
            "1 month" => now.AddMonths(1),
            _ => now.AddDays(1)
        };
    }

    private static List<CommentRequestDto> MapCommentsToDto(List<Comment> comments, HashSet<Guid>? processedIds = null)
    {
        processedIds ??= new HashSet<Guid>();
        var result = new List<CommentRequestDto>();

        foreach (var comment in comments)
        {
            if (processedIds.Contains(comment.Id))
            {
                continue;
            }

            processedIds.Add(comment.Id);

            var dto = MapCommentToDto(comment);

            if (comment.ChildComments != null && comment.ChildComments.Count != 0)
            {
                dto.ChildComments = MapCommentsToDto(comment.ChildComments.ToList(), processedIds);
            }

            result.Add(dto);
        }

        return result;
    }

    private static CommentRequestDto MapCommentToDto(Comment comment)
    {
        return new CommentRequestDto
        {
            Id = comment.Id,
            Name = comment.Name,
            Body = comment.Body,
            ChildComments = new List<CommentRequestDto>()
        };
    }
}