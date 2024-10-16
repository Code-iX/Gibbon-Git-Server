using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Gibbon.Git.Server.Middleware;

public class ResponseLoggingMiddleware(RequestDelegate next, ILogger<ResponseLoggingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ResponseLoggingMiddleware> _logger = logger;

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Path.Value.Contains(".git"))
        {
            await _next(context);
            return;
        }

        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        if (context.Response.StatusCode != 200)
        {
            await _next(context);
            return;
        }
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        _logger.LogDebug("HTTP Response: {0}", text);

        await responseBody.CopyToAsync(originalBodyStream);
    }
}
