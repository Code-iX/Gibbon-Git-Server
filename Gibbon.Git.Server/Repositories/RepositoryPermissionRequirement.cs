using Microsoft.AspNetCore.Authorization;

namespace Gibbon.Git.Server.Repositories;

public record RepositoryPermissionRequirement(RepositoryAccessLevel RequiredAccessLevel) : IAuthorizationRequirement;
