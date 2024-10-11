using System.Text.Json;

using Gibbon.Git.Server.Helpers;
using Gibbon.Git.Server.Services;

namespace Gibbon.Git.Server.Git.GitService.ReceivePackHook.Durability;

/// <summary>
/// Provides at least once execution guarantee to PostPackReceive hook method
/// </summary>
public class ReceivePackRecovery(IHookReceivePack next, IPathResolver pathResolver) : IHookReceivePack
{
    private readonly IHookReceivePack _next = next;
    private readonly IPathResolver _pathResolver = pathResolver;

    public void PrePackReceive(ParsedReceivePack receivePack)
    {
        var serializedPack = JsonSerializer.Serialize(receivePack, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(_pathResolver.GetRecovery("ReceivePack", StringHelper.RemoveIllegalChars($"{receivePack.RepositoryName}.{receivePack.PackId}.pack")), serializedPack);
        _next.PrePackReceive(receivePack);
    }

    public void PostPackReceive(ParsedReceivePack receivePack, GitExecutionResult result)
    {
        ProcessOnePack(receivePack, result);
        RecoverAll(TimeSpan.FromMinutes(5));
    }

    private void ProcessOnePack(ParsedReceivePack receivePack, GitExecutionResult result)
    {
        _next.PostPackReceive(receivePack, result);

        var packFilePath = _pathResolver.GetRecovery("ReceivePack", StringHelper.RemoveIllegalChars($"{receivePack.RepositoryName}.{receivePack.PackId}.pack"));
        if (File.Exists(packFilePath))
        {
            File.Delete(packFilePath);
        }
    }

    public void RecoverAll(TimeSpan inPast)
    {
        var waitingReceivePacks = new List<ParsedReceivePack>();

        var packDir = _pathResolver.GetRecovery("ReceivePack");
        foreach (var packFilePath in Directory.GetFiles(packDir))
        {
            using var fileReader = new StreamReader(packFilePath);
            var packFileData = fileReader.ReadToEnd();
            var parsedPack = JsonSerializer.Deserialize<ParsedReceivePack>(packFileData);
            waitingReceivePacks.Add(parsedPack);
        }

        foreach (var pack in waitingReceivePacks.OrderBy(p => p.Timestamp))
        {
            // execute if the pack has been waiting for X amount of time
            if ((DateTime.Now - pack.Timestamp) < inPast)
            {
                continue;
            }

            // re-parse result file and execute "post" hooks
            // if result file is no longer there then move on
            string correlationId = pack.PackId;
            string repositoryName = pack.RepositoryName;
            var failedPackResultFilePath = _pathResolver.GetRecovery(StringHelper.RemoveIllegalChars($"{repositoryName}.receive-pack.{correlationId}.result"));
            if (File.Exists(failedPackResultFilePath))
            {
                using (var resultFileStream = File.OpenRead(failedPackResultFilePath))
                {
                    var failedPackResult = GitServiceResultParser.ParseResult(resultFileStream);
                    ProcessOnePack(pack, failedPackResult);
                }
                File.Delete(failedPackResultFilePath);
            }
        }
    }
}
