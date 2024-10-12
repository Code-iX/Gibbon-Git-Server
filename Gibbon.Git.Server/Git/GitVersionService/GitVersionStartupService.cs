using System.Diagnostics;
using System.Threading.Tasks;
using Gibbon.Git.Server.Services;

using Microsoft.Extensions.Logging;

namespace Gibbon.Git.Server.Git.GitVersionService;

public class GitVersionStartupService(ILogger<GitVersionStartupService> logger, IProcessService processService, IPathResolver pathResolver, IGitVersionService gitVersionSettings)
    : IStartupService
{
    private readonly ILogger<GitVersionStartupService> _logger = logger;
    private readonly IProcessService _processService = processService;
    private readonly IPathResolver _pathResolver = pathResolver;
    private readonly IGitVersionService _gitVersionSettings = gitVersionSettings;

    public async Task RunAsync()
    {
        if (await IsGitInstalledGlobally() is not null)
        {
            _logger.LogInformation("Global Git installation found.");
            _gitVersionSettings.IsGitAvailable = true;
        }
        else
        {
            _logger.LogWarning("No global Git installation found.");
            _gitVersionSettings.IsGitAvailable = false;
        }
        return;

        var selectedVersion = await SeekVersion();

        if (selectedVersion != null)
        {
            _logger.LogInformation($"Selected git version: {selectedVersion.Version}, Path: {selectedVersion.Path}, Architecture: {selectedVersion.Architecture}, Global: {selectedVersion.IsGlobal}");
        }
        else
        {
            _logger.LogWarning("No suitable git version found.");
        }
    }

    private async Task<GitVersionInfo> SeekVersion()
    {
        var globalGitVersion = await IsGitInstalledGlobally();

        var gitVersions = new List<GitVersionInfo>();

        if (globalGitVersion != null)
        {
            _logger.LogInformation("Global Git Version: {GlobalGitVersion}", globalGitVersion);

            gitVersions.Add(new GitVersionInfo
            {
                Version = globalGitVersion,
                Path = null,
                Architecture = GetSystemArchitecture(),
                IsGlobal = true
            });
        }
        else
        {
            _logger.LogWarning("No global Git installation found.");
        }

        var parentOfRoot = Directory.GetParent(_pathResolver.GetRoot()).FullName;
        var gitsFolderOfParentOfRoot = Directory.GetDirectories(Path.Combine(parentOfRoot, "Gits"));

        foreach (var gitFolder in gitsFolderOfParentOfRoot)
        {
            var gitPath = Path.Combine(gitFolder, "bin", "git.exe");
            var version = await IsGitAvailable(gitPath);

            if (version != null)
            {
                var architecture = GetArchitectureFromPath(gitFolder);
                _logger.LogInformation("Found git version: {Version} ({Architecture})", version, architecture);

                gitVersions.Add(new GitVersionInfo
                {
                    Version = version,
                    Path = gitPath,
                    Architecture = architecture,
                    IsGlobal = false
                });
            }
        }

        var selectedVersion = SelectHighestVersion(gitVersions);
        return selectedVersion;
    }

    private async Task<string> IsGitInstalledGlobally()
    {
        return await IsGitAvailable("git");
    }

    private class GitVersionInfo
    {
        public string Version { get; set; }
        public string Path { get; set; }
        public string Architecture { get; set; }
        public bool IsGlobal { get; set; }
    }

    private string GetArchitectureFromPath(string gitFolder)
    {
        if (gitFolder.Contains("64-bit"))
        {
            return "64-bit";
        }

        if (gitFolder.Contains("32-bit"))
        {
            return "32-bit";
        }

        return "unknown";
    }

    private static string GetSystemArchitecture()
    {
        return Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
    }

    private GitVersionInfo SelectHighestVersion(List<GitVersionInfo> gitVersions)
    {
        var is64BitOS = Environment.Is64BitOperatingSystem;

        var compatibleVersions = gitVersions
            .Where(v => is64BitOS || v.Architecture != "64-bit")
            .ToList();

        var stableVersion = compatibleVersions
            .Where(v => !v.Version.Contains("rc") && !v.Version.Contains("beta"))
            .OrderByDescending(v => Version.Parse(v.Version))
            .ThenBy(ArchitectureSelector)
            .FirstOrDefault();

        if (stableVersion != null)
        {
            return stableVersion;
        }

        return compatibleVersions
            .OrderByDescending(v => Version.Parse(v.Version))
            .ThenBy(ArchitectureSelector)
            .FirstOrDefault();

        int ArchitectureSelector(GitVersionInfo v) => v.Architecture switch
        {
            "64-bit" => 1,
            "32-bit" => 2,
            _ => 3
        };
    }

    public async Task<string> IsGitAvailable(string binPath = "git.exe")
    {
        var info = CreateProcessStartInfo("--version", binPath);
        var result = await _processService.StartProcessAsync(info);

        if (!result.IsSuccess)
            return null;

        return StripVersion(result.Output);
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
            WorkingDirectory = _pathResolver.GetRepositories()
        };
        return info;
    }

    private string StripVersion(string gitVersionOutput)
    {
        var version = gitVersionOutput.Replace("git version ", "").Trim();

        var versionParts = version.Split('.');
        var shortenedVersion = versionParts[0] + "." + versionParts[1] + "." + versionParts[2];

        if (versionParts.Length > 3 && !versionParts[3].Contains("windows") && !versionParts[3].Contains("linux"))
        {
            shortenedVersion += "." + versionParts[3];
        }

        return shortenedVersion;
    }
}
