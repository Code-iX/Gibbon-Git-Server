using System.Threading.Tasks;

namespace Gibbon.Git.Server.Git.GitDownloadService;

public interface IGitDownloadService
{
    Task<bool> EnsureDownloadedAsync();
}