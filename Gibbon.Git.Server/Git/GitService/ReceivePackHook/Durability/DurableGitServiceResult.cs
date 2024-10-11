using System.Threading.Tasks;

namespace Gibbon.Git.Server.Git.GitService.ReceivePackHook.Durability;

/// <summary>
/// provides durability for result of git command execution
/// by writing result of git command to a file
/// </summary>
public class DurableGitServiceResult(IGitService gitService, IRecoveryFilePathBuilder resultFilePathBuilder)
    : IGitService
{
    private readonly IGitService _gitService = gitService;
    private readonly IRecoveryFilePathBuilder _resultFilePathBuilder = resultFilePathBuilder;

    public async Task ExecuteServiceByName(string correlationId, string repositoryName, string serviceName, ExecutionOptions options, Stream inStream, Stream outStream, string userName, int userId)
    {
        if (serviceName == "receive-pack")
        {
            var resultFilePath = _resultFilePathBuilder.GetPathToResultFile(correlationId, repositoryName, serviceName);
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
