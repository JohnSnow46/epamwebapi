using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Auth;
using Gamestore.Entities.Business;

namespace Gamestore.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly GameCatalogDbContext _context;

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
    public IGameRepository Games { get; }
    public IGameGenreRepository GameGenres { get; }
    public IGamePlatformRepository GamePlatforms { get; }
    public IRepository<Genre> Genres { get; }
    public IRepository<Platform> Platforms { get; }
    public IPublisherRepository Publishers { get; }
    public ICommentRepository Comments { get; }
    public IBanRepository Bans { get; }

    // New User Management repositories
    public IUserRepository Users { get; }
    public IRoleRepository Roles { get; }
    public IUserRoleRepository UserRoles { get; }
    public IPermissionRepository Permissions { get; }
    public IRepository<RolePermission> RolePermissions { get; }

    // Orders & Payments
    public IOrderRepository Orders { get; }
    public IOrderGameRepository OrderGames { get; }
    public IPaymentTransactionRepository PaymentTransactions { get; }
    public async Task CompleteAsync()
    {
        await _context.SaveChangesAsync();
    }
}