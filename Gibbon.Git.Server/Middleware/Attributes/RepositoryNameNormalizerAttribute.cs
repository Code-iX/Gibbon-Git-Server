using Gibbon.Git.Server.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Gibbon.Git.Server.Middleware.Attributes;

/// <summary>
/// Applied to a Controller or Action, this attribute will ensure that a repo name has its case corrected to match that in the database.
/// If the name doesn't match anything in the database, then it's returned unchanged.
/// </summary>
public class RepositoryNameNormalizerAttribute(string repositoryNameParameterName) : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var repositoryService = context.HttpContext.RequestServices.GetRequiredService<IRepositoryService>();
        if (context.ActionArguments.TryGetValue(repositoryNameParameterName, out var incomingRepositoryNameParameter))
        {
            var incomingRepositoryName = (string)incomingRepositoryNameParameter;
            var normalizedName = repositoryService.NormalizeRepositoryName(incomingRepositoryName);

            if (normalizedName != incomingRepositoryName)
            {
                // Update the action argument with the normalized name
                context.ActionArguments[repositoryNameParameterName] = normalizedName;
            }
        }

        base.OnActionExecuting(context);
    }
}