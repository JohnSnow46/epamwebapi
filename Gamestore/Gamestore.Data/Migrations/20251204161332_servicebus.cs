using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gamestore.Data.Migrations;

/// <inheritdoc />
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1300 // Element should begin with upper-case letter
public partial class servicebus : Migration
#pragma warning restore SA1300 // Element should begin with upper-case letter
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserNotificationPreferences",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                NotificationMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserNotificationPreferences", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserNotificationPreferences_UserId",
            table: "UserNotificationPreferences",
            column: "UserId");

#pragma warning disable IDE0300 // Simplify collection initialization
#pragma warning disable CA1861 // Avoid constant arrays as arguments
        migrationBuilder.CreateIndex(
            name: "IX_UserNotificationPreferences_UserId_NotificationMethod",
            table: "UserNotificationPreferences",
            columns: new[] { "UserId", "NotificationMethod" },
            unique: true);
#pragma warning restore CA1861 // Avoid constant arrays as arguments
#pragma warning restore IDE0300 // Simplify collection initialization
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserNotificationPreferences");
    }
}
