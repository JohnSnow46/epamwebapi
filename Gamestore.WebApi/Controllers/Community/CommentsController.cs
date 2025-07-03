// WebApi/Controllers/CommentsController.cs - With Authorization
using System.ComponentModel.DataAnnotations;
using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Dto.CommentsDto;
using Gamestore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Gamestore.WebApi.Extensions;

namespace Gamestore.WebApi.Controllers.Community;

[ApiController]
[Route("api")]
public class CommentsController(ICommentService commentService, ILogger<CommentsController> logger) : ControllerBase
{
    private readonly ICommentService _commentService = commentService;
    private readonly ILogger<CommentsController> _logger = logger;

    /// <summary>
    /// Epic 9: Everyone can view comments (read-only for guests)
    /// </summary>
    [HttpGet("games/{key}/comments")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCommentsByGameKey(string key)
    {
        try
        {
            var userInfo = User.Identity?.IsAuthenticated == true
                ? $"{User.GetUserEmail()} (Role: {User.GetUserRole()})"
                : "Anonymous Guest";

            _logger.LogInformation("Getting comments for game with key: {Key} by user: {UserInfo}", key, userInfo);

            var comments = await _commentService.GetCommentsByGameKeyAsync(key);
            return Ok(comments);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error retrieving comments for game with key: {key}");
        }
    }

    /// <summary>
    /// Epic 9: Authenticated users (except Guest) can add comments
    /// </summary>
    [HttpPost("games/{key}/comments")]
    [Authorize(Policy = "CanAddComments")]
    public async Task<IActionResult> AddComment(string key, [FromBody] CommentMetadataRequestDto commentRequest)
    {
        try
        {
            _logger.LogInformation("Adding comment for game with key: {Key} by user: {User} (Role: {Role})",
                key, User.GetUserEmail(), User.GetUserRole());

            if (commentRequest == null)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Comment data is required",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var comment = await _commentService.AddCommentAsync(key, commentRequest);

            _logger.LogInformation("Successfully added comment for game {Key} by user {User}",
                key, User.GetUserEmail());

            return Ok(comment);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error adding comment for game with key: {key}");
        }
    }

    /// <summary>
    /// Epic 9: Admin, Manager, Moderator can delete comments. Users can delete their own comments.
    /// </summary>
    [HttpDelete("games/{key}/comments/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(string key, Guid id)
    {
        try
        {
            // Check if user can moderate comments or if it's their own comment
            if (!User.CanModerateComments())
            {
                // TODO: In real implementation, check if comment belongs to current user
                // For now, we'll only allow moderators and above to delete
                return Forbid("You can only delete your own comments or you need moderation permissions");
            }

            _logger.LogInformation("Deleting comment with ID: {Id} for game with key: {Key} by user: {User} (Role: {Role})",
                id, key, User.GetUserEmail(), User.GetUserRole());

            var comment = await _commentService.DeleteCommentAsync(id);

            _logger.LogInformation("Successfully deleted comment {Id} by user {User}",
                id, User.GetUserEmail());

            return Ok(comment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error deleting comment with ID: {id}");
        }
    }

    /// <summary>
    /// Epic 9: Admin, Manager, Moderator can view ban durations
    /// </summary>
    [HttpGet("comments/ban/durations")]
    [Authorize(Policy = "CanModerateComments")]
    public async Task<IActionResult> GetBanDurations()
    {
        try
        {
            _logger.LogInformation("Getting ban durations for moderator: {User} (Role: {Role})",
                User.GetUserEmail(), User.GetUserRole());

            var durations = await _commentService.GetBanDurationsAsync();
            return Ok(durations);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving ban durations");
        }
    }

    /// <summary>
    /// Epic 9: Admin, Manager, Moderator can ban users
    /// </summary>
    [HttpPost("comments/ban")]
    [Authorize(Policy = "CanBanUsers")]
    public async Task<IActionResult> BanUser([FromBody] BanCreateRequestDto banRequest)
    {
        try
        {
            _logger.LogInformation("Banning user: {User} for duration: {Duration} by moderator: {Moderator} (Role: {Role})",
                banRequest?.User, banRequest?.Duration, User.GetUserEmail(), User.GetUserRole());

            if (banRequest == null)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Ban data is required",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            // Prevent users from banning themselves
            if (string.Equals(banRequest.User, User.GetUserEmail(), StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "You cannot ban yourself",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            await _commentService.BanUserAsync(banRequest);

            _logger.LogInformation("Successfully banned user {BannedUser} by moderator {Moderator}",
                banRequest.User, User.GetUserEmail());

            return Ok(new
            {
                message = $"User {banRequest.User} has been banned for {banRequest.Duration}",
                bannedBy = User.GetUserName(),
                bannedAt = DateTime.UtcNow
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error banning user: {banRequest?.User}");
        }
    }

    /// <summary>
    /// Epic 9: Check if user is banned (internal endpoint for moderators)
    /// </summary>
    [HttpGet("comments/user/{userName}/ban-status")]
    [Authorize(Policy = "CanModerateComments")]
    public async Task<IActionResult> CheckUserBanStatus(string userName)
    {
        try
        {
            _logger.LogInformation("Checking ban status for user: {UserName} by moderator: {Moderator}",
                userName, User.GetUserEmail());

            var isBanned = await _commentService.IsUserBannedAsync(userName);

            return Ok(new
            {
                userName,
                isBanned,
                checkedBy = User.GetUserName(),
                checkedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error checking ban status for user: {userName}");
        }
    }

    private ObjectResult HandleException(Exception ex, string logMessage)
    {
        _logger.LogError(ex, "{LogMessage}: {ErrorMessage}", logMessage, ex.Message);

        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseModel
        {
            Message = "An error occurred.",
            Details = ex.Message,
            StatusCode = StatusCodes.Status500InternalServerError
        });
    }
}