using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Gibbon.Git.Server.Git;

public class GitCmdResult(string contentType, Func<Stream, Task> executeGitCommand, string advertiseRefsContent = null)
    : IActionResult
{
    private readonly string _contentType = contentType;
    private readonly string _advertiseRefsContent = advertiseRefsContent;
    private readonly Func<Stream, Task> _executeGitCommand = executeGitCommand;

    public async Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var response = context.HttpContext.Response;

        response.Headers.Expires = "Fri, 01 Jan 1980 00:00:00 GMT";
        response.Headers.Pragma = "no-cache";
        response.Headers.CacheControl = "no-cache, max-age=0, must-revalidate";

        response.ContentType = _contentType;

        if (!string.IsNullOrEmpty(_advertiseRefsContent))
        {
            await response.WriteAsync(_advertiseRefsContent);
        }

        await _executeGitCommand(response.Body);
    }
}
