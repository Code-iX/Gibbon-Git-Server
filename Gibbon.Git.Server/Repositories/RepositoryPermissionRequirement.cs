using Microsoft.AspNetCore.Authorization;

namespace Gibbon.Git.Server.Repositories;

public class RepositoryPermissionRequirement(RepositoryAccessLevel requiredAccessLevel) : IAuthorizationRequirement
{
    public RepositoryAccessLevel RequiredAccessLevel { get; } = requiredAccessLevel;
}
