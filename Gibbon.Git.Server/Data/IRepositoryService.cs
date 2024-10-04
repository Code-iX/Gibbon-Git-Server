using Gibbon.Git.Server.Models;

namespace Gibbon.Git.Server.Data;

public interface IRepositoryService
{
    List<RepositoryModel> GetAllRepositories();
    List<RepositoryModel> GetTeamRepositories(Guid teamsId);
    RepositoryModel GetRepository(Guid id);
    RepositoryModel GetRepository(string name);
    bool IsAuditPushUser(string name);
    bool Create(RepositoryModel repository);
    void Update(RepositoryModel repository);
    void Delete(Guid id);
    bool NameIsUnique(string newName, Guid ignoreRepoId);

    /// <summary>
    /// Correct a repository name have the same case as it has in the database
    /// If the repo is not in the database, then the name is returned unchanged
    /// </summary>
    string NormalizeRepositoryName(string incomingRepositoryName);
}
