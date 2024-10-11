using System.Diagnostics;
using System.Threading.Tasks;

using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Extensions;
using Gibbon.Git.Server.Git.GitService;
using Gibbon.Git.Server.Security;
using Gibbon.Git.Server.Services;

using Microsoft.Extensions.Options;

namespace Gibbon.Git.Server.Git;

public class GitServiceExecutor : IGitService
{
    private static readonly string[] PermittedServiceNames = ["upload-pack", "receive-pack"];
    private readonly string _gitPath;
    private readonly string _gitHomePath;
    private readonly IProcessService _processService;
    private readonly IPathResolver _pathResolver;
    private readonly ITeamService _teamService;
    private readonly IRoleProvider _roleProvider;
    private readonly IUserService _userService;

    public GitServiceExecutor(IOptions<GitSettings> options, IProcessService processService, IPathResolver pathResolver, ITeamService teamService, IRoleProvider roleProvider, IUserService userService)
    {
        var parameters = options.Value;
        _gitPath = parameters.BinaryPath;
        _gitHomePath = parameters.HomePath;
        _processService = processService;
        _pathResolver = pathResolver;
        _teamService = teamService;
        _roleProvider = roleProvider;
        _userService = userService;
    }

    public async Task ExecuteServiceByName(string correlationId, string repositoryName, string serviceName, ExecutionOptions options, Stream inStream, Stream outStream, string userName, int userId)
    {
        if (!PermittedServiceNames.Contains(serviceName))
        {
            throw new InvalidOperationException("Invalid service name.");
        }

        var args = $"{serviceName} --stateless-rpc{options.ToCommandLineArgs()}";
        args += $" \"{_pathResolver.GetRepositoryPath(repositoryName)}\"";

        var info = CreateProcessStartInfo(args, "git.exe");

        SetUserEnvironment(userName, userId, info);

        await _processService.StartProcessWithStreamAsync(info, inStream, outStream, options.EndStreamWithClose);
    }

    private ProcessStartInfo CreateProcessStartInfo(string args, string gitPath)
    {
        var info = new ProcessStartInfo(gitPath, args)
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _pathResolver.GetRepositories(),
            EnvironmentVariables =
            {
                ["HOME"] = _gitHomePath
            }
        };
        return info;
    }

    private void SetUserEnvironment(string userName, int userId, ProcessStartInfo info)
    {
        var teamsstr = "";
        var rolesstr = "";
        var displayname = "";
        if (!string.IsNullOrEmpty(userName))
        {
            var teams = _teamService.GetTeamsForUser(userId);
            teamsstr = teams.Select(x => x.Name).StringlistToEscapedStringForEnvVar();
            rolesstr = _roleProvider.GetRolesForUser(userId).StringlistToEscapedStringForEnvVar();
            displayname = _userService.GetUserModel(userId).DisplayName;

        }
        // If anonymous option is set then these will always be empty
        info.EnvironmentVariables.Add("AUTH_USER", userName);
        info.EnvironmentVariables.Add("REMOTE_USER", userName);
        info.EnvironmentVariables.Add("AUTH_USER_TEAMS", teamsstr);
        info.EnvironmentVariables.Add("AUTH_USER_ROLES", rolesstr);
        info.EnvironmentVariables.Add("AUTH_USER_DISPLAYNAME", displayname);
    }
}
