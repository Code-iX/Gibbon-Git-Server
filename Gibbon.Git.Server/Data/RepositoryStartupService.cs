using System.Threading.Tasks;
using Gibbon.Git.Server.Services;

namespace Gibbon.Git.Server.Data;

public sealed class RepositoryStartupService(IRepositorySynchronizer repositorySynchronizer)
    : IStartupService
{
    private readonly IRepositorySynchronizer _repositorySynchronizer = repositorySynchronizer;

    public async Task RunAsync()
    {
        _repositorySynchronizer.SynchronizeRepository();
    }
}