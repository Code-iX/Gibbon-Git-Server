using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gibbon.Git.Server.Middleware.Attributes;

public class GitControllerExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var logger = context.HttpContext.RequestServices.GetService<ILogger<GitControllerExceptionFilter>>();

        logger.LogError(context.Exception, "Exception caught in GitController");

        context.Result = new ContentResult
        {
            Content = context.Exception.ToString(),
            StatusCode = 500
        };

        context.HttpContext.Response.Headers.Append("StatusDescription", "Exception in GitController");
        context.HttpContext.Response.Headers.Append("X-Error-Description", "Exception in GitController");

        context.ExceptionHandled = true;
    }
}