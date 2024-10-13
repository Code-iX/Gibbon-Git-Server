using System.Threading.Tasks;

namespace Gibbon.Git.Server.Git.GitService;

/// <summary>
/// Wrapper around git service execution
/// </summary>
public interface IGitService
{
    Task ExecuteServiceByName(string repositoryName, string serviceName, ExecutionOptions options, Stream inStream, Stream outStream, int userId);
}
