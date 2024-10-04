using Gibbon.Git.Server.Models;

namespace Gibbon.Git.Server.Security;

public interface IRepositoryPermissionService
{
    // Used by bonobo
    bool HasPermission(Guid userId, Guid repositoryId, RepositoryAccessLevel requiredLevel);
    bool HasCreatePermission(Guid userId);
    IEnumerable<RepositoryModel> GetAllPermittedRepositories(Guid userId, RepositoryAccessLevel requiredLevel);
    bool HasPermission(Guid userId, string repositoryName, RepositoryAccessLevel requiredLevel);
}
