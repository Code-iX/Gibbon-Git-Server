using Gibbon.Git.Server.Models;

namespace Gibbon.Git.Server.Repositories;

/// <summary>
/// Interface for browsing repository data and providing information about branches, tags, commits, and files.
/// </summary>
public interface IRepositoryBrowser : IDisposable
{
    /// <summary>
    /// Retrieves a list of all branch names in the repository.
    /// </summary>
    /// <returns>A list of branch names.</returns>
    List<string> GetBranches();

    /// <summary>
    /// Retrieves a list of all tag names in the repository.
    /// </summary>
    /// <returns>A list of tag names.</returns>
    List<string> GetTags();

    /// <summary>
    /// Retrieves a list of tags associated with the specified name.
    /// </summary>
    /// <param name="name">The name of the reference to look up.</param>
    /// <param name="page">The page number for pagination.</param>
    /// <param name="p">The number of items per page.</param>
    /// <param name="referenceName">The reference name output parameter.</param>
    /// <param name="totalCount">The total count of tags.</param>
    /// <returns>A list of <see cref="RepositoryCommitModel" /> objects.</returns>
    List<RepositoryCommitModel> GetTags(string name, int page, int p, out string referenceName, out int totalCount);

    /// <summary>
    /// Retrieves a list of commits for the specified name.
    /// </summary>
    /// <param name="name">The name of the reference to look up.</param>
    /// <param name="page">The page number for pagination.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="referenceName">The reference name output parameter.</param>
    /// <param name="totalCount">The total count of commits.</param>
    /// <returns>A list of <see cref="RepositoryCommitModel" /> objects.</returns>
    List<RepositoryCommitModel> GetCommits(string name, int page, int pageSize, out string referenceName, out int totalCount);

    /// <summary>
    /// Retrieves detailed information about a specific commit.
    /// </summary>
    /// <param name="name">The name or ID of the commit.</param>
    /// <returns>A <see cref="RepositoryCommitModel" /> containing detailed information about the commit.</returns>
    RepositoryCommitModel GetCommitDetail(string name);

    /// <summary>
    /// Browses the tree structure of the specified commit.
    /// </summary>
    /// <param name="name">The name of the reference to look up.</param>
    /// <param name="path">The path within the repository to browse.</param>
    /// <param name="referenceName">The reference name output parameter.</param>
    /// <param name="includeDetails">Whether to include detailed information about each item.</param>
    /// <returns>A list of <see cref="RepositoryTreeDetailModel" /> objects.</returns>
    List<RepositoryTreeDetailModel> BrowseTree(string name, string path, out string referenceName, bool includeDetails = false);

    /// <summary>
    /// Browses the content of a blob (file) in the specified commit.
    /// </summary>
    /// <param name="name">The name of the reference to look up.</param>
    /// <param name="path">The path to the blob within the repository.</param>
    /// <param name="referenceName">The reference name output parameter.</param>
    /// <returns>A <see cref="RepositoryTreeDetailModel" /> containing the details of the blob.</returns>
    RepositoryTreeDetailModel BrowseBlob(string name, string path, out string referenceName);

    /// <summary>
    /// Retrieves blame information for a specific file.
    /// </summary>
    /// <param name="name">The name of the reference to look up.</param>
    /// <param name="path">The path to the file within the repository.</param>
    /// <param name="referenceName">The reference name output parameter.</param>
    /// <returns>A <see cref="RepositoryBlameModel" /> containing blame information for the file.</returns>
    RepositoryBlameModel GetBlame(string name, string path, out string referenceName);

    /// <summary>
    /// Retrieves the history of changes for a specific file path.
    /// </summary>
    /// <param name="path">The path to the file within the repository.</param>
    /// <param name="name">The name of the reference to look up.</param>
    /// <param name="referenceName">The reference name output parameter.</param>
    /// <returns>A list of <see cref="RepositoryCommitModel" /> objects representing the commit history.</returns>
    List<RepositoryCommitModel> GetHistory(string path, string name, out string referenceName);

    /// <summary>
    /// Sets the repository path for the <see cref="IRepositoryBrowser" /> instance.
    /// </summary>
    /// <param name="repositoryPath">The path to the repository.</param>
    void SetRepository(string repositoryPath);
}
