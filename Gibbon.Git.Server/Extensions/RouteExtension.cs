using Microsoft.AspNetCore.Builder;

namespace Gibbon.Git.Server.Extensions;

public static class RouteExtension
{
    public static void MapRoutes(this WebApplication app)
    {
        app.MapControllers();

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
            name: "Default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    }
}
