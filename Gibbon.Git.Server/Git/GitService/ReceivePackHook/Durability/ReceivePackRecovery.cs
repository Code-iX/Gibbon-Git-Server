using System.Text.Json;

namespace Gibbon.Git.Server.Git.GitService.ReceivePackHook.Durability;

/// <summary>
/// Provides at least once execution guarantee to PostPackReceive hook method
/// </summary>
public class ReceivePackRecovery(IHookReceivePack next, NamedArguments.FailedPackWaitTimeBeforeExecution failedPackWaitTimeBeforeExecution, IRecoveryFilePathBuilder recoveryFilePathBuilder, GitServiceResultParser resultFileParser) : IHookReceivePack
{
    private readonly TimeSpan _failedPackWaitTimeBeforeExecution = failedPackWaitTimeBeforeExecution.Value;
    private readonly IHookReceivePack _next = next;
    private readonly IRecoveryFilePathBuilder _recoveryFilePathBuilder = recoveryFilePathBuilder;
    private readonly GitServiceResultParser _resultFileParser = resultFileParser;

    public void PrePackReceive(ParsedReceivePack receivePack)
    {
        var serializedPack = JsonSerializer.Serialize(receivePack, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(_recoveryFilePathBuilder.GetPathToPackFile(receivePack), serializedPack);
        _next.PrePackReceive(receivePack);
    }

    public void PostPackReceive(ParsedReceivePack receivePack, GitExecutionResult result)
    {
        ProcessOnePack(receivePack, result);
        RecoverAll();
    }

    private void ProcessOnePack(ParsedReceivePack receivePack, GitExecutionResult result)
    {
        _next.PostPackReceive(receivePack, result);

        var packFilePath = _recoveryFilePathBuilder.GetPathToPackFile(receivePack);
        if (File.Exists(packFilePath))
        {
            File.Delete(packFilePath);
        }
    }

    private void RecoverAll()
    {
        var waitingReceivePacks = new List<ParsedReceivePack>();

        foreach (var packDir in _recoveryFilePathBuilder.GetPathToPackDirectory())
        {
            foreach (var packFilePath in Directory.GetFiles(packDir))
            {
                using var fileReader = new StreamReader(packFilePath);
                var packFileData = fileReader.ReadToEnd();
                var parsedPack = JsonSerializer.Deserialize<ParsedReceivePack>(packFileData);
                waitingReceivePacks.Add(parsedPack);
            }
        }

        foreach (var pack in waitingReceivePacks.OrderBy(p => p.Timestamp))
        {
            // execute if the pack has been waiting for X amount of time
            if ((DateTime.Now - pack.Timestamp) < _failedPackWaitTimeBeforeExecution)
            {
                continue;
            }

            // re-parse result file and execute "post" hooks
            // if result file is no longer there then move on
            var failedPackResultFilePath = _recoveryFilePathBuilder.GetPathToResultFile(pack.PackId, pack.RepositoryName, "receive-pack");
            if (File.Exists(failedPackResultFilePath))
            {
                using (var resultFileStream = File.OpenRead(failedPackResultFilePath))
                {
                    var failedPackResult = _resultFileParser.ParseResult(resultFileStream);
                    ProcessOnePack(pack, failedPackResult);
                }
                File.Delete(failedPackResultFilePath);
            }
        }
    }
}
