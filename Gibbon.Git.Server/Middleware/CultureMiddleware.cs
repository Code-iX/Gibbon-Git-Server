using System.Threading;
using System.Threading.Tasks;
using Gibbon.Git.Server.Extensions;
using Gibbon.Git.Server.Services;
using Microsoft.AspNetCore.Localization;

namespace Gibbon.Git.Server.Middleware;

public class CultureMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, ICultureService cultureService)
    {
        var userId = context.User.Id();

        var cultureInfo = await cultureService.GetSelectedCultureInfo(userId);

        var requestCulture = new RequestCulture(cultureInfo);
        context.Features.Set<IRequestCultureFeature>(new RequestCultureFeature(requestCulture, null));

        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;

        await _next(context);
    }
}
