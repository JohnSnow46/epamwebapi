using Gamestore.Entities.Auth;
using Gamestore.Entities.Business;
using Gamestore.Entities.Notifications;

namespace Gamestore.Data.Interfaces;

public interface IUnitOfWork
{
    // Existing repositories
    IGameRepository Games { get; }

    IGameGenreRepository GameGenres { get; }

    IGamePlatformRepository GamePlatforms { get; }

    IRepository<Genre> Genres { get; }

    IRepository<Platform> Platforms { get; }

    IPublisherRepository Publishers { get; }

    ICommentRepository Comments { get; }

    IBanRepository Bans { get; }

    // New User Management repositories
    IUserRepository Users { get; }

    IRoleRepository Roles { get; }

    IUserRoleRepository UserRoles { get; }

    IPermissionRepository Permissions { get; }

    IRepository<RolePermission> RolePermissions { get; }

    // User notification
    IUserNotificationRepository UserNotifications { get; }

    IRepository<Order> Orders { get; }

    IRepository<OrderDetail> OrderDetails { get; }

    IRepository<OrderNotification> OrderNotifications { get; }

    Task CompleteAsync();
}
