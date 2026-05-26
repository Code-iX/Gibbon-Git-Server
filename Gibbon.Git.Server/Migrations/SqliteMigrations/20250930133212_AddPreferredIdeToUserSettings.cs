using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gibbon.Git.Server.Migrations.SqliteMigrations
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
                type: "INTEGER",
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
