using System.Threading.Tasks;
using Gibbon.Git.Server.Git.Models;

namespace Gibbon.Git.Server.Git.GitService;

/// <summary>
/// Wrapper around git service execution
/// </summary>
public interface IGitService
{
    Task ExecuteServiceByName(string correlationId, string repositoryName, string serviceName, ExecutionOptions options, Stream inStream, Stream outStream, string userName, int userId);
}
