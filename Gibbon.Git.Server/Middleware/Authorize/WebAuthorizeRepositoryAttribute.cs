using System.Security.Claims;

using Gibbon.Git.Server.Security;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace Gibbon.Git.Server.Middleware.Authorize;

public class WebAuthorizeRepositoryAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    public bool RequiresRepositoryAdministrator { get; set; }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.User.Identity!.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!context.HttpContext.User.IsInRole(Security.Roles.Member))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var roles = Roles?.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (roles is null)
        {
            return;
        }

        if (!context.HttpContext.User.Claims.Any(c => roles.Contains(c.Value)))
        {
            context.Result = new UnauthorizedResult();
        }
        if (context.Result != null) return;

        var repositoryPermissionService = context.HttpContext.RequestServices.GetRequiredService<IRepositoryPermissionService>();
        var tempDataFactory = context.HttpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();
        var tempData = tempDataFactory.GetTempData(context.HttpContext);

        if (int.TryParse(context.RouteData.Values["id"]?.ToString(), out var repoId))
        {
            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userId, out var parsedUserId))
            {
                var requiredAccess = RequiresRepositoryAdministrator ? RepositoryAccessLevel.Administer : RepositoryAccessLevel.Push;

                if (!repositoryPermissionService.HasPermission(parsedUserId, repoId, requiredAccess))
                {
                    context.Result = new UnauthorizedResult();
                }
            }
        }
        else
        {
            tempData["RepositoryNotFound"] = true;
            context.Result = new RedirectToActionResult("Index", "Repository", null);
        }
    }
}
