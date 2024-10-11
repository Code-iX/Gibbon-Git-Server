using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gibbon.Git.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Repository",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false, collation: "NOCASE"),
                    Group = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Anonymous = table.Column<bool>(type: "INTEGER", nullable: false),
                    Logo = table.Column<byte[]>(type: "BLOB", nullable: true),
                    AllowAnonymousPush = table.Column<int>(type: "INTEGER", nullable: false),
                    AuditPushUser = table.Column<bool>(type: "INTEGER", nullable: false),
                    LinksRegex = table.Column<string>(type: "TEXT", nullable: true),
                    LinksUrl = table.Column<string>(type: "TEXT", nullable: true),
                    LinksUseGlobal = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repository", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false, collation: "NOCASE"),
                    Description = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AllowAnonymousPush = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowUserRepositoryCreation = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowPushToCreate = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowAnonymousRegistration = table.Column<bool>(type: "INTEGER", nullable: false),
                    DefaultLanguage = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    SiteTitle = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SiteLogoUrl = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    SiteCssUrl = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    IsCommitAuthorAvatarVisible = table.Column<bool>(type: "INTEGER", nullable: false),
                    LinksRegex = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    LinksUrl = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Team",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false, collation: "NOCASE"),
                    Description = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Team", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Surname = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false, collation: "NOCASE"),
                    Password = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PasswordSalt = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    PreferredLanguage = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    PreferredThemeMode = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceiveEmailNotifications = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeZone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DateFormat = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    DefaultHomePage = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "TeamRepository_Permission",
                columns: table => new
                {
                    Repository_Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Team_Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamRepository_Permission", x => new { x.Repository_Id, x.Team_Id });
                    table.ForeignKey(
                        name: "FK_TeamRepository_Permission_Repository_Repository_Id",
                        column: x => x.Repository_Id,
                        principalTable: "Repository",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamRepository_Permission_Team_Team_Id",
                        column: x => x.Team_Id,
                        principalTable: "Team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRepository_Administrator",
                columns: table => new
                {
                    Repository_Id = table.Column<int>(type: "INTEGER", nullable: false),
                    User_Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRepository_Administrator", x => new { x.Repository_Id, x.User_Id });
                    table.ForeignKey(
                        name: "FK_UserRepository_Administrator_Repository_Repository_Id",
                        column: x => x.Repository_Id,
                        principalTable: "Repository",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRepository_Administrator_User_User_Id",
                        column: x => x.User_Id,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRepository_Permission",
                columns: table => new
                {
                    Repository_Id = table.Column<int>(type: "INTEGER", nullable: false),
                    User_Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRepository_Permission", x => new { x.Repository_Id, x.User_Id });
                    table.ForeignKey(
                        name: "FK_UserRepository_Permission_Repository_Repository_Id",
                        column: x => x.Repository_Id,
                        principalTable: "Repository",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRepository_Permission_User_User_Id",
                        column: x => x.User_Id,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRole_InRole",
                columns: table => new
                {
                    Role_Id = table.Column<int>(type: "INTEGER", nullable: false),
                    User_Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole_InRole", x => new { x.Role_Id, x.User_Id });
                    table.ForeignKey(
                        name: "FK_UserRole_InRole_Role_Role_Id",
                        column: x => x.Role_Id,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRole_InRole_User_User_Id",
                        column: x => x.User_Id,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTeam_Member",
                columns: table => new
                {
                    Team_Id = table.Column<int>(type: "INTEGER", nullable: false),
                    User_Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTeam_Member", x => new { x.Team_Id, x.User_Id });
                    table.ForeignKey(
                        name: "FK_UserTeam_Member_Team_Team_Id",
                        column: x => x.Team_Id,
                        principalTable: "Team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTeam_Member_User_User_Id",
                        column: x => x.User_Id,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 1, "System administrator", "Administrator" });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Email", "Name", "Password", "PasswordSalt", "Surname", "Username" },
                values: new object[] { 1, "", "admin", "2dpBKPc2rPqPa03udauh6LUo4uNHFSNQZaH4P1BIkNizmUmuir/61Vgkr5MaXlr+bVWnefxQD1H1ciMEtEr/hQ==", "/4fKgvYmp7iCSD7JJMPhrw==", "", "admin" });

            migrationBuilder.InsertData(
                table: "UserRole_InRole",
                columns: new[] { "Role_Id", "User_Id" },
                values: new object[] { 1, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Repository_Name",
                table: "Repository",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Role_Name",
                table: "Role",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Team_Name",
                table: "Team",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamRepository_Permission_Team_Id",
                table: "TeamRepository_Permission",
                column: "Team_Id");

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRepository_Administrator_User_Id",
                table: "UserRepository_Administrator",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserRepository_Permission_User_Id",
                table: "UserRepository_Permission",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_InRole_User_Id",
                table: "UserRole_InRole",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserTeam_Member_User_Id",
                table: "UserTeam_Member",
                column: "User_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerSettings");

            migrationBuilder.DropTable(
                name: "TeamRepository_Permission");

            migrationBuilder.DropTable(
                name: "UserRepository_Administrator");

            migrationBuilder.DropTable(
                name: "UserRepository_Permission");

            migrationBuilder.DropTable(
                name: "UserRole_InRole");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "UserTeam_Member");

            migrationBuilder.DropTable(
                name: "Repository");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "Team");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
