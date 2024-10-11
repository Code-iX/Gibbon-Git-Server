namespace Gibbon.Git.Server.Services;

public interface IPathResolver
{
    string Resolve(params string[] path);
    string ResolveRoot(string path);

    string GetRepositories();

    string GetRepoPath(string path, string applicationPath);
    string GetRepositoryPath(string repositoryName);
    string GetRoot();
    string GetRecovery(params string[] appends);
}
