using Gibbon.Git.Server.Extensions;
using Gibbon.Git.Server.Helpers;
using Gibbon.Git.Server.Models;
using Gibbon.Git.Server.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Gibbon.Git.Server.Middleware.Attributes;

public class AllViewsFilter(IRepositoryPermissionService repoPermissions, IUrlHelperFactory urlHelperFactory)
    : IActionFilter
{
    private readonly IRepositoryPermissionService _repoPermissions = repoPermissions;
    private readonly IUrlHelperFactory _urlHelperFactory = urlHelperFactory;

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var userId = context.HttpContext.User.Id();
        if (context.Controller is Controller controller)
        {
            controller.ViewData["PermittedRepositories"] = PopulateRepoGoToList(userId, context);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    private List<SelectListItem> PopulateRepoGoToList(int userId, ActionExecutingContext context)
    {
        var pullList = _repoPermissions.GetAllPermittedRepositories(userId, RepositoryAccessLevel.Pull);
        var adminList = _repoPermissions.GetAllPermittedRepositories(userId, RepositoryAccessLevel.Administer);
        var firstList = pullList.Union(adminList, InlineComparer<RepositoryModel>.Create((lhs, rhs) => lhs.Id == rhs.Id, obj => obj.Id.GetHashCode()))
            .OrderBy(x => x.Name.ToLowerInvariant())
            .GroupBy(x => x.Group ?? Resources.Repository_No_Group);

        var items = new List<SelectListItem>();
        var urlHelper = _urlHelperFactory.GetUrlHelper(context);

        var groups = new Dictionary<string, SelectListGroup>();
        foreach (var grouped in firstList)
        {
            if (!groups.TryGetValue(grouped.Key, out var group))
            {
                group = new SelectListGroup { Name = grouped.Key };
                groups[grouped.Key] = group;
            }

            foreach (var item in grouped)
            {
                var selectListItem = new SelectListItem
                {
                    Text = item.Name,
                    Value = urlHelper.Action("Detail", "Repository", new { id = item.Id }),
                    Group = group,
                    Selected = item.Id == userId
                };

                items.Add(selectListItem);
            }
        }

        return items;
    }
}
