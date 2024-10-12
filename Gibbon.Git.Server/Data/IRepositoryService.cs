using Gibbon.Git.Server.Models;

namespace Gibbon.Git.Server.Data;

public interface IRepositoryService
{
    List<RepositoryModel> GetAllRepositories();
    List<RepositoryModel> GetTeamRepositories(int teamsId);
    RepositoryModel GetRepository(int id);
    RepositoryModel GetRepository(string name);
    bool Create(RepositoryModel repository);
    void Update(RepositoryModel repository);
    void Delete(int id);
    bool NameIsUnique(string newName, int ignoreRepoId);

    /// <summary>
    /// Correct a repository name have the same case as it has in the database
    /// If the repo is not in the database, then the name is returned unchanged
    /// </summary>
    string NormalizeRepositoryName(string repositoryName);
}
