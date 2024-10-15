using System.Threading.Tasks;
using Gibbon.Git.Server.Extensions;
using Gibbon.Git.Server.Security;
using Microsoft.AspNetCore.Authorization;

namespace Gibbon.Git.Server.Repositories;

public class RepositoryPermissionHandler(IRepositoryPermissionService repositoryPermissionService) : AuthorizationHandler<RepositoryPermissionRequirement>
{
    private readonly IRepositoryPermissionService _repositoryPermissionService = repositoryPermissionService;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RepositoryPermissionRequirement requirement)
    {
        if (!context.User.Identity.IsAuthenticated)
        {
            context.Fail();
            return;
        }

        if (context.Resource is not HttpContext routeData || !int.TryParse(routeData.Request.RouteValues["id"]?.ToString(), out var repoId))
        {
            context.Fail();
            return;
        }

        var userId = context.User.Id();
        if (!_repositoryPermissionService.HasPermission(userId, repoId, requirement.RequiredAccessLevel))
        {
            context.Fail();
            return;
        }

        context.Succeed(requirement);
    }
}
