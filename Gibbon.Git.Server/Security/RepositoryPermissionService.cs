using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Models;

using Microsoft.Extensions.Logging;

namespace Gibbon.Git.Server.Security;

public class RepositoryPermissionService(IRepositoryService repository, IRoleProvider roleProvider, ITeamService teamService, ILogger<RepositoryPermissionService> logger, ServerSettings serverSettings)
    : IRepositoryPermissionService
{
    private readonly ServerSettings _serverSettings = serverSettings;
    private readonly ILogger<RepositoryPermissionService> _logger = logger;
    private readonly IRepositoryService _repository = repository;
    private readonly IRoleProvider _roleProvider = roleProvider;
    private readonly ITeamService _teamService = teamService;

    public bool HasPermission(int userId, string repositoryName, RepositoryAccessLevel requiredLevel)
    {
        var repository = _repository.GetRepository(repositoryName);
        return repository != null && HasPermission(userId, repository.Id, requiredLevel);
    }

    public bool HasPermission(int userId, int repositoryId, RepositoryAccessLevel requiredLevel)
    {
        var repositoryModel = _repository.GetRepository(repositoryId);
        if (repositoryModel == null)
        {
            throw new InvalidOperationException("Repository not found.");
        }
        return HasPermission(userId, _teamService.GetTeamsForUser(userId), repositoryModel, requiredLevel);
    }

    public bool HasCreatePermission(int userId)
    {
        if (userId == 0)
        {
            // Anonymous users cannot create repos
            return false;
        }
        return IsSystemAdministrator(userId) || _serverSettings.AllowUserRepositoryCreation;
    }

    public IEnumerable<RepositoryModel> GetAllPermittedRepositories(int userId, RepositoryAccessLevel requiredLevel)
    {
        var userTeams = _teamService.GetTeamsForUser(userId);
        return _repository.GetAllRepositories().Where(repo => HasPermission(userId, userTeams, repo, requiredLevel));
    }

    private bool HasPermission(int userId, List<TeamModel> userTeams, RepositoryModel repositoryModel, RepositoryAccessLevel requiredLevel)
    {
        // All users can take advantage of the anonymous permissions
        if (CheckAnonymousPermission(repositoryModel, requiredLevel))
        {
            _logger.LogDebug(
                "Permitting user {UserId} anonymous permission {Permission} on repo {RepositoryName}",
                userId,
                requiredLevel,
                repositoryModel.Name);
            return true;
        }
        if (userId == 0)
        {
            // We have no named user
            return false;
        }

        // Named users have more possibilities
        return CheckNamedUserPermission(userId, userTeams, repositoryModel, requiredLevel);
    }

    private bool CheckAnonymousPermission(RepositoryModel repository, RepositoryAccessLevel requiredLevel)
    {
        if (!repository.AnonymousAccess)
        {
            // There's no anon access at all to this repo
            return false;
        }

        return requiredLevel switch
        {
            RepositoryAccessLevel.Pull => true,
            RepositoryAccessLevel.Push => repository.AllowAnonymousPush == RepositoryPushMode.Yes || (repository.AllowAnonymousPush == RepositoryPushMode.Global && _serverSettings.AllowAnonymousPush),
            RepositoryAccessLevel.Administer => false,
            _ => throw new ArgumentOutOfRangeException(nameof(requiredLevel), requiredLevel, null)
        };
    }

    private bool CheckNamedUserPermission(int userId, List<TeamModel> userTeams, RepositoryModel repository, RepositoryAccessLevel requiredLevel)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, 0, nameof(userId));

        var userIsAnAdministrator = IsSystemAdministrator(userId) || repository.Administrators.Any(x => x.Id == userId);

        _logger.LogTrace("Checking user {UserId} (admin? {IsAdmin}) has permission {Permission} on repo {RepositoryName}",
             userId,
             userIsAnAdministrator,
             requiredLevel,
             repository.Name);

        return requiredLevel switch
        {
            RepositoryAccessLevel.Push or RepositoryAccessLevel.Pull => userIsAnAdministrator || UserIsARepoUser(userId, repository) || UserIsATeamMember(userTeams, repository),
            RepositoryAccessLevel.Administer => userIsAnAdministrator,
            _ => throw new ArgumentOutOfRangeException(nameof(requiredLevel), requiredLevel, null)
        };
    }

    private static bool UserIsARepoUser(int userId, RepositoryModel repository)
    {
        return repository.Users.Any(x => x.Id == userId);
    }

    private static bool UserIsATeamMember(List<TeamModel> userTeams, RepositoryModel repository)
    {
        return userTeams
            .Any(x => repository.Teams.Select(y => y.Name)
            .Contains(x.Name, StringComparer.OrdinalIgnoreCase));
    }

    private bool IsSystemAdministrator(int userId)
    {
        return _roleProvider
            .GetRolesForUser(userId)
            .Contains(Definitions.Roles.Administrator);
    }
}
