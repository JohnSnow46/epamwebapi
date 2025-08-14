using Gamestore.Entities.Auth;
using Gamestore.Entities.Business;
using Gamestore.Entities.Community;
using Gamestore.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Data;

/// <summary>
/// Database context for the Game Catalog application, providing access to all entities
/// and their relationships including games, users, orders, and authentication data.
/// </summary>
public class GameCatalogDbContext(DbContextOptions<GameCatalogDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the Games entity set for managing game records.
    /// </summary>
    public DbSet<Game> Games { get; set; }

    /// <summary>
    /// Gets or sets the Genres entity set for managing game genre records.
    /// </summary>
    public DbSet<Genre> Genres { get; set; }

    /// <summary>
    /// Gets or sets the Platforms entity set for managing gaming platform records.
    /// </summary>
    public DbSet<Platform> Platforms { get; set; }

    /// <summary>
    /// Gets or sets the GameGenres entity set for managing many-to-many relationships between games and genres.
    /// </summary>
    public DbSet<GameGenre> GameGenres { get; set; }

    /// <summary>
    /// Gets or sets the GamePlatforms entity set for managing many-to-many relationships between games and platforms.
    /// </summary>
    public DbSet<GamePlatform> GamePlatforms { get; set; }

    /// <summary>
    /// Gets or sets the Publishers entity set for managing game publisher records.
    /// </summary>
    public DbSet<Publisher> Publishers { get; set; }

    /// <summary>
    /// Gets or sets the Comments entity set for managing user comments on games.
    /// </summary>
    public DbSet<Comment> Comments { get; set; }

    /// <summary>
    /// Gets or sets the Bans entity set for managing user ban records.
    /// </summary>
    public DbSet<Ban> Bans { get; set; }

    // New DbSets for User Management
    /// <summary>
    /// Gets or sets the Users entity set for managing user account records.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Gets or sets the Roles entity set for managing user role definitions.
    /// </summary>
    public DbSet<Role> Roles { get; set; }

    /// <summary>
    /// Gets or sets the UserRoles entity set for managing many-to-many relationships between users and roles.
    /// </summary>
    public DbSet<UserRole> UserRoles { get; set; }

    /// <summary>
    /// Gets or sets the Permissions entity set for managing system permission definitions.
    /// </summary>
    public DbSet<Permission> Permissions { get; set; }

    /// <summary>
    /// Gets or sets the RolePermissions entity set for managing many-to-many relationships between roles and permissions.
    /// </summary>
    public DbSet<RolePermission> RolePermissions { get; set; }

    // Orders & Payments
    /// <summary>
    /// Gets or sets the Orders entity set for managing customer order records.
    /// </summary>
    public DbSet<Order> Orders { get; set; }

    /// <summary>
    /// Gets or sets the OrderGames entity set for managing many-to-many relationships between orders and games.
    /// </summary>
    public DbSet<OrderGame> OrderGames { get; set; }

    /// <summary>
    /// Gets or sets the PaymentTransactions entity set for managing payment transaction records.
    /// </summary>
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

    /// <summary>
    /// Configures the database model using Entity Framework's Fluent API.
    /// Defines relationships, constraints, indexes, and other database schema configurations.
    /// </summary>
    /// <param name="modelBuilder">The ModelBuilder instance used to configure the database model.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Game
        modelBuilder.Entity<Game>()
            .HasKey(g => g.Id);

        modelBuilder.Entity<Game>()
            .Property(g => g.Name)
            .IsRequired();

        modelBuilder.Entity<Game>()
            .Property(g => g.Key)
            .IsRequired();

        modelBuilder.Entity<Game>()
            .HasIndex(g => g.Key)
            .IsUnique();

        modelBuilder.Entity<Game>()
            .Property(g => g.Price)
            .IsRequired();

        modelBuilder.Entity<Game>()
            .Property(g => g.UnitInStock)
            .IsRequired();

        modelBuilder.Entity<Game>()
            .Property(g => g.Discontinued)
            .IsRequired();

        // Game-Publisher
        modelBuilder.Entity<Game>()
            .HasOne(g => g.Publisher)
            .WithMany(p => p.Games)
            .HasForeignKey(g => g.PublisherId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        // Publisher
        modelBuilder.Entity<Publisher>()
          .HasKey(p => p.Id);

        modelBuilder.Entity<Publisher>()
            .Property(p => p.CompanyName)
            .IsRequired();

        modelBuilder.Entity<Publisher>()
            .HasIndex(p => p.CompanyName)
            .IsUnique();

        modelBuilder.Entity<GameGenre>()
            .HasKey(gg => new { gg.GameId, gg.GenreId });

        modelBuilder.Entity<GamePlatform>()
            .HasKey(gp => new { gp.GameId, gp.PlatformId });

        // GameGenre
        modelBuilder.Entity<GameGenre>()
            .HasOne(gg => gg.Game)
            .WithMany(g => g.GameGenres)
            .HasForeignKey(gg => gg.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GameGenre>()
            .HasOne(gg => gg.Genre)
            .WithMany(g => g.GameGenres)
            .HasForeignKey(gg => gg.GenreId)
            .OnDelete(DeleteBehavior.Cascade);

        // GamePlatform
        modelBuilder.Entity<GamePlatform>()
            .HasOne(gp => gp.Game)
            .WithMany(g => g.GamePlatforms)
            .HasForeignKey(gp => gp.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GamePlatform>()
            .HasOne(gp => gp.Platform)
            .WithMany(p => p.GamePlatforms)
            .HasForeignKey(gp => gp.PlatformId)
            .OnDelete(DeleteBehavior.Cascade);

        // Comment
        modelBuilder.Entity<Comment>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<Comment>()
            .Property(c => c.Name)
            .IsRequired();

        modelBuilder.Entity<Comment>()
            .Property(c => c.Body)
            .IsRequired();

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Game)
            .WithMany()
            .HasForeignKey(c => c.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.ParentComment)
            .WithMany(c => c.ChildComments)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Epic 8 - MongoDB integration fields dla Publisher
        modelBuilder.Entity<Publisher>()
            .Property(p => p.ContactName)
            .HasMaxLength(50);

        modelBuilder.Entity<Publisher>()
            .Property(p => p.Phone)
            .HasMaxLength(30);

        modelBuilder.Entity<Publisher>()
            .Property(p => p.Country)
            .HasMaxLength(50);
    }
}