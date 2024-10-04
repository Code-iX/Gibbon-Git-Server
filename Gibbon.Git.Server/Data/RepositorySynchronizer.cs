using Gibbon.Git.Server.Models;
using Gibbon.Git.Server.Services;

namespace Gibbon.Git.Server.Data;

public sealed class RepositorySynchronizer(IRepositoryService repositoryService, IPathResolver pathResolver)
    : IRepositorySynchronizer
{
    private readonly IRepositoryService _repositoryService = repositoryService;
    private readonly IPathResolver _pathResolver = pathResolver;

    public void SynchronizeRepository()
    {
        if (!Directory.Exists(_pathResolver.GetRepositories()))
        {
            // We don't want an exception if the repo dir no longer exists, 
            // as this would make it impossible to start the server
            return;
        }
        var directories = Directory.EnumerateDirectories(_pathResolver.GetRepositories());
        foreach (var directory in directories)
        {
            CheckDirectory(directory);
        }
    }

    private void CheckDirectory(string directory)
    {
        var name = Path.GetFileName(directory);

        var repository = _repositoryService.GetRepository(name);
        if (repository != null)
            return;

        if (!LibGit2Sharp.Repository.IsValid(directory))
            return;

        repository = new RepositoryModel
        {
            Id = Guid.NewGuid(),
            Description = "Discovered in file system.",
            Name = name,
            AnonymousAccess = false
        };
        if (repository.NameIsValid)
        {
            _repositoryService.Create(repository);
        }
    }
}
