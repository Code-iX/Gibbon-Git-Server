using Gibbon.Git.Server.Configuration;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Gibbon.Git.Server.Services;

internal class PathResolver(IWebHostEnvironment hostingEnvironment, IOptions<ApplicationSettings> configuration)
    : IPathResolver
{
    private readonly IWebHostEnvironment _hostingEnvironment = hostingEnvironment;
    private readonly ApplicationSettings _appSettings = configuration.Value;

    public string Resolve(params string[] path)
    {
        ArgumentNullException.ThrowIfNull(path);
        var first = path[0];
        var latter = path[1..];

        if (Path.IsPathRooted(first))
        {
            return ResolveCore(first, latter);
        }

        return ResolveCore(_hostingEnvironment.ContentRootPath, [_appSettings.DataPath, .. path]);
    }
    public string ResolveRoot(string path)
    {
        if (Path.IsPathRooted(path))
        {
            return path;
        }

        return ResolveCore(_hostingEnvironment.ContentRootPath, path);
    }

    public string GetRepositories() => ResolveRoot(_appSettings.RepositoryPath);

    public string GetRepoPath(string path, string applicationPath)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (!string.IsNullOrEmpty(applicationPath))
        {
            path = path.Replace(applicationPath, "");
        }

        var gitIndex = path.IndexOf(".git", StringComparison.Ordinal);
        if (gitIndex == -1)
            return path;

        return path[..gitIndex].TrimStart('/');
    }

    public string GetRepositoryPath(string repositoryName)
    {
        ArgumentNullException.ThrowIfNull(repositoryName);

        return Path.Combine(GetRepositories(), repositoryName);
    }

    public string GetRoot()
    {
        return _hostingEnvironment.ContentRootPath;
    }

    public string GetRecovery(params string[] paths)
    {
        return ResolveCore(_appSettings.RecoveryPath, paths);
    }

    internal static string ResolveCore(string rootPath, params string[] paths)
    {
        ArgumentNullException.ThrowIfNull(rootPath);
        ArgumentNullException.ThrowIfNull(paths);

        if (paths.Length == 0)
        {
            return rootPath;
        }

        var appendPath = paths[0];

        appendPath = appendPath.TrimStart('~', '/', '\\');

        if (string.IsNullOrEmpty(rootPath))
        {
            rootPath = appendPath;
        }
        else if (!string.IsNullOrEmpty(appendPath))
        {
            rootPath = Path.Combine(rootPath, appendPath);
        }

        rootPath = ResolveCore(rootPath, paths[1..]);

        return rootPath;
    }
}
