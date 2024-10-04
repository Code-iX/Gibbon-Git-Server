using Microsoft.AspNetCore.Builder;

namespace Gibbon.Git.Server.Extensions;

public static class RouteExtension
{
    public static void MapRoutes(this WebApplication app)
    {
        app.MapDefaultControllerRoute();

        app.MapControllerRoute(
            name: "HomeActions",
            pattern: "{action=Index}",
            defaults: new { controller = "Home" }
        );

        app.MapControllerRoute(
            name: "AccountWithUsername",
            pattern: "Account/Me",
            defaults: new { controller = "Account", action = "Edit" }
        );

        app.MapControllerRoute(
            name: "AccountWithId",
            pattern: "Account/{id}/{action}",
            defaults: new { controller = "Account", action = "Detail" },
            constraints: new { id = @"\d+" });

        app.MapControllerRoute(
            name: "Team",
            pattern: "Team/{id}/{action}/{teamname?}",
            defaults: new { controller = "Team", action = "Detail" },
            constraints: new { id = @"\d+" });

        app.MapControllerRoute(
            name: "Validation",
            pattern: "Validation/{action}",
            defaults: new { controller = "Validation", action = string.Empty });

        app.MapControllerRoute(
                name: "SecureInfoRefs",
                pattern: "{repositoryName}.git/info/refs",
                defaults: new { controller = "Git", action = "SecureGetInfoRefs" })
            .WithMetadata(new HttpMethodMetadata(["GET"]));

        app.MapControllerRoute(
                name: "SecureUploadPack",
                pattern: "{repositoryName}.git/git-upload-pack",
                defaults: new { controller = "Git", action = "SecureUploadPack" })
            .WithMetadata(new HttpMethodMetadata(["POST"]));

        app.MapControllerRoute(
                name: "SecureReceivePack",
                pattern: "{repositoryName}.git/git-receive-pack",
                defaults: new { controller = "Git", action = "SecureReceivePack" })
            .WithMetadata(new HttpMethodMetadata(["POST"]));

        app.MapControllerRoute(
                name: "GitBaseUrl",
                pattern: "{repositoryName}.git",
                defaults: new { controller = "Git", action = "GitUrl" })
            .WithMetadata(new HttpMethodMetadata(["GET"]));

        app.MapControllerRoute(
            name: "RepositoryTree",
            pattern: "Repository/{id}/{encodedName}/Tree/{*encodedPath}",
            defaults: new { controller = "Repository", action = "Tree" });

        app.MapControllerRoute(
            name: "RepositoryBlob",
            pattern: "Repository/{id}/{encodedName}/Blob/{*encodedPath}",
            defaults: new { controller = "Repository", action = "Blob" },
            constraints: new { id = @"\d+" });

        app.MapControllerRoute(
            name: "RepositoryRaw",
            pattern: "Repository/{id}/{encodedName}/Raw/{*encodedPath}",
            defaults: new { controller = "Repository", action = "Raw" });

        app.MapControllerRoute(
            name: "RepositoryBlame",
            pattern: "Repository/{id}/{encodedName}/Blame/{*encodedPath}",
            defaults: new { controller = "Repository", action = "Blame" });

        app.MapControllerRoute(
            name: "RepositoryDownload",
            pattern: "Repository/{id}/{encodedName}/Download/{*encodedPath}",
            defaults: new { controller = "Repository", action = "Download" });

        app.MapControllerRoute(
            name: "RepositoryCommits",
            pattern: "Repository/{id}/{encodedName}/Commits",
            defaults: new { controller = "Repository", action = "Commits" });

        app.MapControllerRoute(
            name: "RepositoryCommit",                         
            pattern: "Repository/{id}/{encodedName}/Commit/{commit}/",
            defaults: new { controller = "Repository", action = "Commit" });

        app.MapControllerRoute(
            name: "RepositoryHistory",
            pattern: "Repository/{id}/{encodedName}/History/{*encodedPath}",
            defaults: new { controller = "Repository", action = "History" },
            constraints: new { id = @"\d+" });

        app.MapControllerRoute(
            name: "Repository",
            pattern: "Repository/{id}/{action}/{reponame?}",
            defaults: new { controller = "Repository", action = "Detail" },
            constraints: new { id = @"\d+" });

        app.MapControllerRoute(
            name: "RepoCommits",
            pattern: "Repository/Commits/{id}",
            defaults: new { controller = "Repository", action = "Commits", id = string.Empty });

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    }
}
