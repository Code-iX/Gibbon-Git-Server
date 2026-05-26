using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gibbon.Git.Server.Migrations.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class AddPreferredIdeToUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreferredIde",
                table: "UserSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredIde",
                table: "UserSettings");
        }
    }
}
