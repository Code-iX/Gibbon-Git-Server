using System.Net.Http;
using System.Threading.Tasks;

using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gibbon.Git.Server.Git.GitDownloadService;

internal class GitDownloadService(ILogger<GitDownloadService> logger, IOptions<GitSettings> options, HttpClient httpClient, IPathResolver pathResolver)
    : IGitDownloadService
{
    private readonly HttpClient _httpClient = httpClient;

    private readonly GitSettings _gitSettings = options.Value;

    private readonly string _gitDir = pathResolver.ResolveRoot(options.Value.BinaryPath);

    private readonly ILogger<GitDownloadService> _logger = logger;

    public async Task<bool> EnsureDownloadedAsync()
    {
        if (!await DownloadGitAsync()) 
            return false;
        if (!await UnpackGitAsync()) 
            return false;
        await RunPostInstallAsync();
        CleanUpAsync();
        return true;
    }

    private async Task<bool> DownloadGitAsync()
    {
        try
        {
            var gitPackagePath = Path.Combine(_gitDir, $"PortableGit-{_gitSettings.Version}-{_gitSettings.Architecture}-bit.7z.exe");

            var gitExePath = Path.Combine(_gitDir, "bin", "git.exe");
            if (File.Exists(gitExePath))
            {
                _logger.LogInformation("Git is already installed.");
                return true;
            }

            Directory.CreateDirectory(_gitDir);

            if (File.Exists(gitPackagePath))
            {
                _logger.LogInformation("GIT package file already downloaded.");
                return true;
            }

            var downloadUrl = ConstructGitDownloadUrl();

            _logger.LogInformation($"Downloading Git from {downloadUrl} to {gitPackagePath}");
            using (var response = await _httpClient.GetAsync(downloadUrl))
            {
                response.EnsureSuccessStatusCode();
                await using (var fs = new FileStream(gitPackagePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fs);
                }
            }

            _logger.LogInformation("Git successfully downloaded.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading Git.");
            return false;
        }
    }

    private async Task<bool> UnpackGitAsync()
    {
        try
        {
            var gitPackagePath = Path.Combine(_gitDir, $"PortableGit-{_gitSettings.Version}-{_gitSettings.Architecture}-bit.7z.exe");
            var gitVersionDir = Path.Combine(_gitDir, _gitSettings.Version);

            if (!File.Exists(gitPackagePath))
            {
                _logger.LogWarning("Git package file not found.");
                return false;
            }

            Directory.CreateDirectory(gitVersionDir);

            var arguments = $"-nr -y -gm2 -InstallPath=\"{gitVersionDir}\"";

            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = gitPackagePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = System.Diagnostics.Process.Start(processInfo))
            {
                if (process == null)
                {
                    _logger.LogError("Error starting process.");
                    return false;
                }

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (!string.IsNullOrWhiteSpace(output))
                {
                    _logger.LogInformation(output);
                }

                if (!string.IsNullOrWhiteSpace(error))
                {
                    _logger.LogError(error);
                }

                if (process.ExitCode != 0)
                {
                    _logger.LogError($"Error unpacking Git. Exit code: {process.ExitCode}");
                    return false;
                }
            }

            _logger.LogInformation($"Git successfully unpacked to {gitVersionDir}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unpacking.");
            return false;
        }
    }

    private async Task RunPostInstallAsync()
    {
        try
        {
            var gitVersionDir = Path.Combine(_gitDir, _gitSettings.Version);
            var postInstallPath = Path.Combine(gitVersionDir, "post-install.bat");

            if (File.Exists(postInstallPath))
            {
                _logger.LogInformation("Running post-install script.");
                var processInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/c " + postInstallPath)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                var process = System.Diagnostics.Process.Start(processInfo);
                await process.WaitForExitAsync();

                _logger.LogInformation("Post-install script executed.");
            }
            else
            {
                _logger.LogWarning("Post-install script not found.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running post-install script.");
        }
    }

    private void CleanUpAsync()
    {
        try
        {
            var gitPackagePath = Path.Combine(_gitDir, $"PortableGit-{_gitSettings.Version}-{_gitSettings.Architecture}-bit.7z.exe");

            if (File.Exists(gitPackagePath))
            {
                _logger.LogInformation($"Deleting {gitPackagePath}");
                File.Delete(gitPackagePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up.");
        }
    }

    private string ConstructGitDownloadUrl()
    {
        var parts = _gitSettings.Version.Split('.');
        var versionFolder = $"{parts[0]}.{parts[1]}.{parts[2]}.windows.{(_gitSettings.Version.Count(c => c == '.') > 2 ? parts[3] : "1")}";
        var downloadUrl = $"https://github.com/git-for-windows/git/releases/download/v{versionFolder}/PortableGit-{_gitSettings.Version}-{_gitSettings.Architecture}-bit.7z.exe";

        return downloadUrl;
    }
}
