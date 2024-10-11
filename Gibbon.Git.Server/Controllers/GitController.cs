using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Extensions;
using Gibbon.Git.Server.Git;
using Gibbon.Git.Server.Git.GitService;
using Gibbon.Git.Server.Middleware.Attributes;
using Gibbon.Git.Server.Middleware.Authorize;
using Gibbon.Git.Server.Models;
using Gibbon.Git.Server.Security;
using Gibbon.Git.Server.Services;

using ICSharpCode.SharpZipLib.GZip;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Repository = LibGit2Sharp.Repository;

namespace Gibbon.Git.Server.Controllers;

[ServiceFilter(typeof(GitControllerExceptionFilter))]
[RepositoryNameNormalizer("repositoryName")]
[GitAuthorize]
public class GitController(ILogger<GitController> logger, IRepositoryPermissionService repositoryPermissionService, IRepositoryService repositoryService, IUserService userService, IGitService gitService, ServerSettings serverOptions, IPathResolver pathResolver)
    : Controller
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
        var isPush = string.Equals("git-receive-pack", service, StringComparison.OrdinalIgnoreCase);

        if (!RepositoryIsValid(repositoryName))
        {
            // This isn't a real repo - but we might consider allowing creation
            if (isPush && _serverSettings.AllowPushToCreate)
            {
                if (!_repositoryPermissionService.HasCreatePermission(User.Id()))
                {
                    _logger.LogWarning("User {UserId} is not allowed to do push-to-create", User.Id());
                    return Unauthorized();
                }
                if (!TryCreateOnPush(repositoryName))
                {
                    return Unauthorized();
                }
            }
            else
            {
                return NotFound();
            }
        }

        var requiredLevel = isPush ? RepositoryAccessLevel.Push : RepositoryAccessLevel.Pull;
        if (!_repositoryPermissionService.HasPermission(User.Id(), repositoryName, requiredLevel))
        {
            _logger.LogWarning("SecureGetInfoRefs unauth because User {UserId} doesn't have permission {Permission} on  repo {RepositoryName}", User.Id(), requiredLevel, repositoryName);
            return Unauthorized();
        }

        return GetInfoRefs(repositoryName, service);
    }

    [HttpPost]
    public IActionResult SecureUploadPack(string repositoryName)
    {
        if (!RepositoryIsValid(repositoryName))
        {
            return NotFound();
        }

        if (!_repositoryPermissionService.HasPermission(User.Id(), repositoryName, RepositoryAccessLevel.Pull))
        {
            return Unauthorized();
        }

        return ExecuteUploadPack(repositoryName);
    }

    [HttpPost]
    public IActionResult SecureReceivePack(string repositoryName)
    {
        if (!RepositoryIsValid(repositoryName))
        {
            return NotFound();
        }

        if (!_repositoryPermissionService.HasPermission(User.Id(), repositoryName, RepositoryAccessLevel.Push))
        {
            return Unauthorized();
        }

        return ExecuteReceivePack(repositoryName);
    }

    private bool TryCreateOnPush(string repositoryName)
    {
        var directory = GetDirectoryInfo(repositoryName);
        if (directory.Exists)
        {
            // We can't create a new repo - there's already a directory with that name
            _logger.LogWarning("Can't create {RepositoryName} - directory already exists", repositoryName);
            return false;
        }
        var repository = new RepositoryModel
        {
            Name = repositoryName
        };
        if (!repository.NameIsValid)
        {
            // We don't like this name
            _logger.LogWarning("Can't create '{RepositoryName}' - name is invalid", repositoryName);
            return false;
        }
        var user = _userService.GetUserModel(User.Id());
        repository.Description = "Auto-created by push for " + user.DisplayName;
        repository.AnonymousAccess = false;
        repository.Administrators = [user];
        if (!_repositoryService.Create(repository))
        {
            // We can't add this to the repo store
            _logger.LogWarning("Can't create '{RepositoryName}' - RepoRepo.Create failed", repositoryName);
            return false;
        }

        Repository.Init(Path.Combine(_pathResolver.GetRepositories(), repository.Name), true);
        _logger.LogInformation("'{RepositoryName}' created", repositoryName);
        return true;
    }

    private IActionResult ExecuteReceivePack(string repositoryName)
    {
        var userName = HttpContext.User.Username();
        var userId = HttpContext.User.Id();
        return new GitCmdResult(
            "application/x-git-receive-pack-result",
            async outStream => await _gitService.ExecuteServiceByName(
                Guid.NewGuid().ToString("N"),
                repositoryName,
                "receive-pack",
                new ExecutionOptions(false),
                GetInputStream(disableBuffer: true),
                outStream,
                userName,
                userId
            )
        );
    }

    private IActionResult ExecuteUploadPack(string repositoryName)
    {
        var userName = HttpContext.User.Username();
        var userId = HttpContext.User.Id();
        return new GitCmdResult(
            "application/x-git-upload-pack-result",
            async outStream => await _gitService.ExecuteServiceByName(
                Guid.NewGuid().ToString("N"),
                repositoryName,
                "upload-pack",
                new ExecutionOptions(false, true),
                GetInputStream(),
                outStream,
                userName,
                userId
            )
        );
    }

    private IActionResult GetInfoRefs(string repositoryName, string service)
    {
        Response.StatusCode = 200;

        var userName = HttpContext.User.Username();
        var userId = HttpContext.User.Id();
        return new GitCmdResult(
            $"application/x-{service}-advertisement",
            async outStream =>
            {
                await _gitService.ExecuteServiceByName(Guid.NewGuid().ToString("N"),
                    repositoryName,
                    service[4..],
                    new ExecutionOptions(true),
                    GetInputStream(),
                    outStream,
                    userName,
                    userId);
            },
            CreateFormattedServiceMessage(service));
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

    private Stream GetInputStream(bool disableBuffer = false)
    {
        // For really large uploads we need to get a bufferless input stream and disable the max
        // request length.
        var requestStream = HttpContext.Request.Body;

        return Request.Headers["Content-Encoding"] == "gzip"
            ? new GZipInputStream(requestStream)
            : requestStream;
    }
}
