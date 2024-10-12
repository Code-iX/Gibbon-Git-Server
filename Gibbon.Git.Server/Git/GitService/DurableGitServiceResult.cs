using System.Threading.Tasks;
using Gibbon.Git.Server.Git.Models;
using Gibbon.Git.Server.Helpers;
using Gibbon.Git.Server.Services;

namespace Gibbon.Git.Server.Git.GitService;

/// <summary>
/// provides durability for result of git command execution
/// by writing result of git command to a file
/// </summary>
public class DurableGitServiceResult(IGitService gitService, IPathResolver pathResolver)
    : IGitService
{
    private readonly IGitService _gitService = gitService;
    private readonly IPathResolver _pathResolver = pathResolver;

    public async Task ExecuteServiceByName(string correlationId, string repositoryName, string serviceName, ExecutionOptions options, Stream inStream, Stream outStream, string userName, int userId)
    {
        if (serviceName == "receive-pack")
        {
            var resultFilePath = _pathResolver.GetRecovery(StringHelper.RemoveIllegalChars($"{repositoryName}.{serviceName}.{correlationId}.result"));
            await using (var resultFileStream = File.OpenWrite(resultFilePath))
            {
                await _gitService.ExecuteServiceByName(correlationId, repositoryName, serviceName, options, inStream, new ReplicatingStream(outStream, resultFileStream), userName, userId);
            }

            // only on successful execution remove the result file
            if (File.Exists(resultFilePath))
            {
                File.Delete(resultFilePath);
            }
        }
        else
        {
            await _gitService.ExecuteServiceByName(correlationId, repositoryName, serviceName, options, inStream, outStream, userName, userId);
        }
    }
}
