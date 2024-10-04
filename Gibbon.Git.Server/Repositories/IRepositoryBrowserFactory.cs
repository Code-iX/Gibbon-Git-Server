namespace Gibbon.Git.Server.Repositories;

/// <summary>
/// Factory interface for creating instances of <see cref="IRepositoryBrowser" />.
/// </summary>
public interface IRepositoryBrowserFactory
{
    /// <summary>
    /// Creates an instance of <see cref="IRepositoryBrowser" /> for the specified repository path.
    /// </summary>
    /// <param name="repositoryPath">The path to the repository.</param>
    /// <returns>An instance of <see cref="IRepositoryBrowser" />.</returns>
    IRepositoryBrowser Create(string repositoryPath);
}
