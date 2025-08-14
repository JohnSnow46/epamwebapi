using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Auth;
using Gamestore.Entities.Business;

namespace Gamestore.Data.Repositories;

/// <summary>
/// Unit of Work implementation that coordinates multiple repository operations within a single transaction.
/// Implements the Unit of Work pattern to maintain consistency across related data operations
/// and provides centralized access to all repository instances in the application.
/// Ensures atomic operations and proper transaction management across the entire data layer.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly GameCatalogDbContext _context;

    /// <summary>
    /// Initializes a new instance of the UnitOfWork class with the specified database context.
    /// Creates and configures all repository instances for the application's data entities,
    /// organizing them into logical groups for business operations, user management, and e-commerce.
    /// </summary>
    /// <param name="context">The Entity Framework database context to share across all repositories.</param>
    public UnitOfWork(GameCatalogDbContext context)
    {
        _context = context;

        // Existing repositories
        Games = new GameRepository(_context);
        GameGenres = new GameGenreRepository(_context);
        GamePlatforms = new GamePlatformRepository(_context);
        Genres = new Repository<Genre>(_context);
        Platforms = new Repository<Platform>(_context);
        Publishers = new PublisherRepository(_context);
        Comments = new CommentRepository(_context);
        Bans = new BanRepository(_context);

        // New User Management repositories
        Users = new UserRepository(_context);
        Roles = new RoleRepository(_context);
        UserRoles = new UserRoleRepository(_context);
        Permissions = new PermissionRepository(_context);
        RolePermissions = new Repository<RolePermission>(_context);

        // Orders & Payments
        Orders = new OrderRepository(_context);
        OrderGames = new OrderGameRepository(_context);
        PaymentTransactions = new PaymentTransactionRepository(_context);
    }

    // Existing repositories
    /// <summary>
    /// Gets the repository for managing Game entities and game-specific operations.
    /// </summary>
    public IGameRepository Games { get; }

    /// <summary>
    /// Gets the repository for managing GameGenre relationships and genre associations.
    /// </summary>
    public IGameGenreRepository GameGenres { get; }

    /// <summary>
    /// Gets the repository for managing GamePlatform relationships and platform associations.
    /// </summary>
    public IGamePlatformRepository GamePlatforms { get; }

    /// <summary>
    /// Gets the generic repository for managing Genre entities.
    /// </summary>
    public IRepository<Genre> Genres { get; }

    /// <summary>
    /// Gets the generic repository for managing Platform entities.
    /// </summary>
    public IRepository<Platform> Platforms { get; }

    /// <summary>
    /// Gets the repository for managing Publisher entities and publisher-specific operations.
    /// </summary>
    public IPublisherRepository Publishers { get; }

    /// <summary>
    /// Gets the repository for managing Comment entities and comment hierarchy operations.
    /// </summary>
    public ICommentRepository Comments { get; }

    /// <summary>
    /// Gets the repository for managing Ban entities and user ban operations.
    /// </summary>
    public IBanRepository Bans { get; }

    // New User Management repositories
    /// <summary>
    /// Gets the repository for managing User entities and user account operations.
    /// </summary>
    public IUserRepository Users { get; }

    /// <summary>
    /// Gets the repository for managing Role entities and role-based access control.
    /// </summary>
    public IRoleRepository Roles { get; }

    /// <summary>
    /// Gets the repository for managing UserRole relationships and user-role assignments.
    /// </summary>
    public IUserRoleRepository UserRoles { get; }

    /// <summary>
    /// Gets the repository for managing Permission entities and authorization permissions.
    /// </summary>
    public IPermissionRepository Permissions { get; }

    /// <summary>
    /// Gets the generic repository for managing RolePermission relationships.
    /// </summary>
    public IRepository<RolePermission> RolePermissions { get; }

    // Orders & Payments
    /// <summary>
    /// Gets the repository for managing Order entities and order lifecycle operations.
    /// </summary>
    public IOrderRepository Orders { get; }

    /// <summary>
    /// Gets the repository for managing OrderGame relationships and order item operations.
    /// </summary>
    public IOrderGameRepository OrderGames { get; }

    /// <summary>
    /// Gets the repository for managing PaymentTransaction entities and payment processing operations.
    /// </summary>
    public IPaymentTransactionRepository PaymentTransactions { get; }

    /// <summary>
    /// Commits all pending changes made through the repositories within this unit of work.
    /// This method ensures that all operations are executed as a single atomic transaction,
    /// maintaining data consistency across all related entities and relationships.
    /// Should be called after completing a logical set of operations to persist changes.
    /// </summary>
    /// <returns>A task representing the asynchronous commit operation.</returns>
    public async Task CompleteAsync()
    {
        await _context.SaveChangesAsync();
    }
}