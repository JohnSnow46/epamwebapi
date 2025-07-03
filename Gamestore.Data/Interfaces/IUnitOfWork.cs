using Gamestore.Entities.Auth;
using Gamestore.Entities.Business;

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

    // Orders & Payments
    IOrderRepository Orders { get; }
    IOrderGameRepository OrderGames { get; }
    IPaymentTransactionRepository PaymentTransactions { get; }

    Task CompleteAsync();
}
