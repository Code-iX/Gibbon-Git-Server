using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Extensions;
using Gibbon.Git.Server.Git.GitService;
using Gibbon.Git.Server.Middleware.Attributes;
using Gibbon.Git.Server.Middleware.Authorize;
using Gibbon.Git.Server.Repositories;
using Gibbon.Git.Server.Security;
using Gibbon.Git.Server.Services;

using ICSharpCode.SharpZipLib.GZip;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Repository = LibGit2Sharp.Repository;

namespace Gibbon.Git.Server.Controllers;

[GitAuthorize]
public class GitController(ILogger<GitController> logger, IRepositoryPermissionService repositoryPermissionService, IRepositoryService repositoryService, IUserService userService, IGitService gitService, ServerSettings serverOptions, IPathResolver pathResolver)
    : ControllerBase
{
    private readonly ServerSettings _serverSettings = serverOptions;
    private readonly ILogger<GitController> _logger = logger;
    private readonly IRepositoryPermissionService _repositoryPermissionService = repositoryPermissionService;
    private readonly IRepositoryService _repositoryService = repositoryService;
    private readonly IUserService _userService = userService;
    private readonly IGitService _gitService = gitService;
    private readonly IPathResolver _pathResolver = pathResolver;

    public IActionResult SecureGetInfoRefs(string repositoryName, string service)
    {
        if (!RepositoryIsValid(repositoryName))
        {
            return GitNotFound();
        }
        var isPush = string.Equals("git-receive-pack", service, StringComparison.OrdinalIgnoreCase);
        var requiredLevel = isPush ? RepositoryAccessLevel.Push : RepositoryAccessLevel.Pull;
        if (!_repositoryPermissionService.HasPermission(User.Id(), repositoryName, requiredLevel))
        {
            _logger.LogWarning("SecureGetInfoRefs unauth because User {UserId} doesn't have permission {Permission} on  repo {RepositoryName}", User.Id(), requiredLevel, repositoryName);
            return GitForbidden();
        }

        Response.StatusCode = 200;

        return new GitCmdResult(
            $"application/x-{service}-advertisement",
            async outStream => await _gitService.ExecuteServiceByName(repositoryName,
                service[4..],
                new ExecutionOptions(true),
                GetInputStream(),
                outStream,
                HttpContext.User.Id()
            ),
            CreateFormattedServiceMessage(service));
    }

    [HttpPost]
    public IActionResult SecureUploadPack(string repositoryName)
    {
        if (!RepositoryIsValid(repositoryName))
        {
            return GitNotFound();
        }

        if (!_repositoryPermissionService.HasPermission(User.Id(), repositoryName, RepositoryAccessLevel.Pull))
        {
            return GitForbidden();
        }

        return new GitCmdResult(
            "application/x-git-upload-pack-result",
            async outStream => await _gitService.ExecuteServiceByName(repositoryName,
                "upload-pack",
                new ExecutionOptions(false, true),
                GetInputStream(),
                outStream,
                HttpContext.User.Id()
            )
        );
    }

    [HttpPost]
    public IActionResult SecureReceivePack(string repositoryName)
    {
        if (!RepositoryIsValid(repositoryName))
        {
            return GitNotFound();
        }

        if (!_repositoryPermissionService.HasPermission(User.Id(), repositoryName, RepositoryAccessLevel.Push))
        {
            return GitForbidden();
        }

        return new GitCmdResult(
            "application/x-git-receive-pack-result",
            async outStream => await _gitService.ExecuteServiceByName(repositoryName,
                "receive-pack",
                new ExecutionOptions(false),
                GetInputStream(),
                outStream,
                HttpContext.User.Id()
            )
        );
    }

    /// <summary>
    /// Action to handle .git URLs by redirecting to the repository details page.
    /// </summary>
    public IActionResult GitUrl(string repositoryName)
    {
        if (string.IsNullOrEmpty(repositoryName))
        {
            return BadRequest("Repository name is required.");
        }

        return RedirectToAction("Detail", "Repository", new { id = repositoryName });
    }

    private static string CreateFormattedServiceMessage(string service)
    {
        string input = $"# service={service}\n";
        return $"{(input.Length + 4).ToString("X").PadLeft(4, '0')}{input}0000";
    }

    private DirectoryInfo GetDirectoryInfo(string repositoryName)
    {
        return new DirectoryInfo(Path.Combine(_pathResolver.GetRepositories(), repositoryName));
    }

    private bool RepositoryIsValid(string repositoryName)
    {
        var directory = GetDirectoryInfo(repositoryName);
        var isValid = Repository.IsValid(directory.FullName);
        if (!isValid)
        {
            _logger.LogWarning("Invalid repo {RepositoryName}", repositoryName);
        }
        return isValid;
    }

    private Stream GetInputStream()
    {
        // For really large uploads we need to get a bufferless input stream and disable the max
        // request length.
        var requestStream = HttpContext.Request.Body;

        return Request.Headers["Content-Encoding"] == "gzip"
            ? new GZipInputStream(requestStream)
            : requestStream;
    }

    /// <summary>
    /// Returns a plain text response with status code 403 (Forbidden).
    /// </summary>
    /// <returns>A plain text response indicating the request is forbidden.</returns>
    private IActionResult GitForbidden()
    {
        Response.StatusCode = 403;
        return Content("Forbidden.", "text/plain; charset=UTF-8");
    }

    /// <summary>
    /// Returns a plain text response with status code 404 (Not Found).
    /// </summary>
    /// <returns>A plain text response indicating the repository was not found.</returns>
    private IActionResult GitNotFound()
    {
        Response.StatusCode = 404;
        return Content("Repository not found.", "text/plain; charset=UTF-8");
    }
}
