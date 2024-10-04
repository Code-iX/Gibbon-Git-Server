using System.Security.Claims;
using Gibbon.Git.Server.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace Gibbon.Git.Server.Middleware.Authorize;

public class WebAuthorizeRepositoryAttribute : WebAuthorizeAttribute
{
    public bool RequiresRepositoryAdministrator { get; set; }

    public override void OnAuthorization(AuthorizationFilterContext context)
    {
        base.OnAuthorization(context);

        if (context.Result != null) return;

        var repositoryPermissionService = context.HttpContext.RequestServices.GetRequiredService<IRepositoryPermissionService>();
        var tempDataFactory = context.HttpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();
        var tempData = tempDataFactory.GetTempData(context.HttpContext);

        if (Guid.TryParse(context.RouteData.Values["id"]?.ToString(), out var repoId))
        {
            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var requiredAccess = RequiresRepositoryAdministrator ? RepositoryAccessLevel.Administer : RepositoryAccessLevel.Push;

            if (!repositoryPermissionService.HasPermission(Guid.Parse(userId), repoId, requiredAccess))
            {
                context.Result = new UnauthorizedResult();
            }
        }
        else
        {
            tempData["RepositoryNotFound"] = true;
            context.Result = new RedirectToActionResult("Index", "Repository", null);
        }
    }
}