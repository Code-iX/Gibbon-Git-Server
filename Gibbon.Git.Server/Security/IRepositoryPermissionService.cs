using Gibbon.Git.Server.Models;

namespace Gibbon.Git.Server.Security;

public interface IRepositoryPermissionService
{
    // Used by bonobo
    bool HasPermission(int userId, int repositoryId, RepositoryAccessLevel requiredLevel);
    bool HasCreatePermission(int userId);
    IEnumerable<RepositoryModel> GetAllPermittedRepositories(int userId, RepositoryAccessLevel requiredLevel);
    bool HasPermission(int userId, string repositoryName, RepositoryAccessLevel requiredLevel);
}
