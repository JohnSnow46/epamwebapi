using Gamestore.Entities.Auth;
using Gamestore.Entities.Business;

namespace Gamestore.Data.Interfaces;

/// <summary>
/// Unit of Work interface that coordinates multiple repository operations within a single transaction.
/// Implements the Unit of Work pattern to maintain consistency across related data operations
/// and provides centralized access to all repository instances in the application.
/// Ensures atomic operations and proper transaction management across the entire data layer.
/// </summary>
public interface IUnitOfWork
{
    // Existing repositories
    /// <summary>
    /// Gets the repository for managing Game entities and game-specific operations.
    /// </summary>
    IGameRepository Games { get; }

    /// <summary>
    /// Gets the repository for managing GameGenre relationships and genre associations.
    /// </summary>
    IGameGenreRepository GameGenres { get; }

    /// <summary>
    /// Gets the repository for managing GamePlatform relationships and platform associations.
    /// </summary>
    IGamePlatformRepository GamePlatforms { get; }

    /// <summary>
    /// Gets the generic repository for managing Genre entities.
    /// </summary>
    IRepository<Genre> Genres { get; }

    /// <summary>
    /// Gets the generic repository for managing Platform entities.
    /// </summary>
    IRepository<Platform> Platforms { get; }

    /// <summary>
    /// Gets the repository for managing Publisher entities and publisher-specific operations.
    /// </summary>
    IPublisherRepository Publishers { get; }

    /// <summary>
    /// Gets the repository for managing Comment entities and comment hierarchy operations.
    /// </summary>
    ICommentRepository Comments { get; }

    /// <summary>
    /// Gets the repository for managing Ban entities and user ban operations.
    /// </summary>
    IBanRepository Bans { get; }

    // New User Management repositories
    /// <summary>
    /// Gets the repository for managing User entities and user account operations.
    /// </summary>
    IUserRepository Users { get; }

    /// <summary>
    /// Gets the repository for managing Role entities and role-based access control.
    /// </summary>
    IRoleRepository Roles { get; }

    /// <summary>
    /// Gets the repository for managing UserRole relationships and user-role assignments.
    /// </summary>
    IUserRoleRepository UserRoles { get; }

    /// <summary>
    /// Gets the repository for managing Permission entities and authorization permissions.
    /// </summary>
    IPermissionRepository Permissions { get; }

    /// <summary>
    /// Gets the generic repository for managing RolePermission relationships.
    /// </summary>
    IRepository<RolePermission> RolePermissions { get; }

    // Orders & Payments
    /// <summary>
    /// Gets the repository for managing Order entities and order lifecycle operations.
    /// </summary>
    IOrderRepository Orders { get; }

    /// <summary>
    /// Gets the repository for managing OrderGame relationships and order item operations.
    /// </summary>
    IOrderGameRepository OrderGames { get; }

    /// <summary>
    /// Gets the repository for managing PaymentTransaction entities and payment processing operations.
    /// </summary>
    IPaymentTransactionRepository PaymentTransactions { get; }

    /// <summary>
    /// Gets the repository for managing PaymentMethod entities and payment method operations.
    /// </summary>
    IPaymentMethodRepository PaymentMethods { get; }

    /// <summary>
    /// Commits all pending changes made through the repositories within this unit of work.
    /// This method ensures that all operations are executed as a single atomic transaction,
    /// maintaining data consistency across all related entities and relationships.
    /// </summary>
    /// <returns>A task representing the asynchronous commit operation.</returns>
    Task CompleteAsync();
}