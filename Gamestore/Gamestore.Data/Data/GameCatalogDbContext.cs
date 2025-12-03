using Gamestore.Entities.Auth;
using Gamestore.Entities.Business;
using Gamestore.Entities.Community;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Data;

public class GameCatalogDbContext(DbContextOptions<GameCatalogDbContext> options) : DbContext(options)
{
    // Existing DbSets
    public DbSet<Game> Games { get; set; }

    public DbSet<Genre> Genres { get; set; }

    public DbSet<Platform> Platforms { get; set; }

    public DbSet<GameGenre> GameGenres { get; set; }

    public DbSet<GamePlatform> GamePlatforms { get; set; }

    public DbSet<Publisher> Publishers { get; set; }

    public DbSet<Comment> Comments { get; set; }

    public DbSet<Ban> Bans { get; set; }

    // New DbSets for User Management
    public DbSet<User> Users { get; set; }

    public DbSet<Role> Roles { get; set; }

    public DbSet<UserRole> UserRoles { get; set; }

    public DbSet<Permission> Permissions { get; set; }

    public DbSet<RolePermission> RolePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ========== EXISTING CONFIGURATIONS (unchanged) ==========

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

        // Epic 11: Performance Indexes

        // Index 1
        modelBuilder.Entity<Game>()
            .HasIndex(g => g.PublisherId)
            .HasDatabaseName("IX_Games_PublisherId");

        // Index 2
        modelBuilder.Entity<Game>()
            .HasIndex(g => g.Discontinued)
            .HasDatabaseName("IX_Games_Discontinued");

        // Index 3
        modelBuilder.Entity<GameGenre>()
            .HasIndex(gg => gg.GenreId)
            .HasDatabaseName("IX_GameGenres_GenreId");

        // Index 4
        modelBuilder.Entity<GamePlatform>()
            .HasIndex(gp => gp.PlatformId)
            .HasDatabaseName("IX_GamePlatforms_PlatformId");

        // Game Image
        modelBuilder.Entity<Game>()
            .Property(g => g.ImageUrl)
            .IsRequired(false)
            .HasMaxLength(2048);

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
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Ban
        modelBuilder.Entity<Ban>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<Ban>()
            .Property(b => b.UserName)
            .IsRequired();

        modelBuilder.Entity<Ban>()
            .HasIndex(b => b.UserName);

        // Additional properties for filtering/sorting
        modelBuilder.Entity<Game>()
            .Property(g => g.ViewCount)
            .HasDefaultValue(0)
            .IsRequired();

        modelBuilder.Entity<Game>()
            .Property(g => g.CommentCount)
            .HasDefaultValue(0)
            .IsRequired();

        modelBuilder.Entity<Game>()
            .Property(g => g.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();

        // ========== NEW USER MANAGEMENT CONFIGURATIONS ==========

        // User
        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<User>()
            .Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<User>()
            .Property(u => u.PasswordHash)
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(u => u.IsActive)
            .HasDefaultValue(true);

        modelBuilder.Entity<User>()
            .Property(u => u.IsEmailConfirmed)
            .HasDefaultValue(false);

        modelBuilder.Entity<User>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Role
        modelBuilder.Entity<Role>()
            .HasKey(r => r.Id);

        modelBuilder.Entity<Role>()
            .Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .Property(r => r.Description)
            .HasMaxLength(500);

        modelBuilder.Entity<Role>()
            .Property(r => r.Level)
            .IsRequired();

        modelBuilder.Entity<Role>()
            .Property(r => r.IsSystemRole)
            .HasDefaultValue(false);

        modelBuilder.Entity<Role>()
            .Property(r => r.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // UserRole (Many-to-Many with additional properties)
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => ur.Id);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique();

        modelBuilder.Entity<UserRole>()
            .Property(ur => ur.AssignedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Permission
        modelBuilder.Entity<Permission>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Permission>()
            .Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Permission>()
            .HasIndex(p => p.Name)
            .IsUnique();

        modelBuilder.Entity<Permission>()
            .Property(p => p.Description)
            .HasMaxLength(500);

        modelBuilder.Entity<Permission>()
            .Property(p => p.Category)
            .HasMaxLength(50);

        // RolePermission (Many-to-Many)
        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => rp.Id);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RolePermission>()
            .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
            .IsUnique();

        modelBuilder.Entity<RolePermission>()
            .Property(rp => rp.GrantedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        base.OnModelCreating(modelBuilder);
    }
}