using Gibbon.Git.Server.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Gibbon.Git.Server.Middleware.Authorize;

public class WebAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    public virtual void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.User.Identity!.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!context.HttpContext.User.IsInRole(Definitions.Roles.Member))
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
    }
}
