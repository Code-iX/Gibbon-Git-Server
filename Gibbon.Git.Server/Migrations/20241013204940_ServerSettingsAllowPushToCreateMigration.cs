using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gibbon.Git.Server.Migrations
{
    /// <inheritdoc />
    public partial class ServerSettingsAllowPushToCreateMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowPushToCreate",
                table: "ServerSettings");

            migrationBuilder.DropColumn(
                name: "AuditPushUser",
                table: "Repository");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowPushToCreate",
                table: "ServerSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AuditPushUser",
                table: "Repository",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
