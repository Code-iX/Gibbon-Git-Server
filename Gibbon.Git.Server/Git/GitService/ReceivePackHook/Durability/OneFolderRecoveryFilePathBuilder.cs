using Gibbon.Git.Server.Helpers;

namespace Gibbon.Git.Server.Git.GitService.ReceivePackHook.Durability;

/// <summary>
/// Generates paths all going into one configured folder
/// </summary>
public class OneFolderRecoveryFilePathBuilder(NamedArguments.ReceivePackRecoveryDirectory receivePackRecoveryDirectory) : IRecoveryFilePathBuilder
{
    private readonly string _receivePackRecoveryDirectory = receivePackRecoveryDirectory.Value;

    public string GetPathToResultFile(string correlationId, string repositoryName, string serviceName)
    {
        var path = $"{repositoryName}.{serviceName}.{correlationId}.result";

        return Path.Combine(_receivePackRecoveryDirectory, StringHelper.RemoveIllegalChars(path));
    }

    public string GetPathToPackFile(ParsedReceivePack receivePack)
    {
        return Path.Combine(_receivePackRecoveryDirectory, "ReceivePack", StringHelper.RemoveIllegalChars($"{receivePack.RepositoryName}.{receivePack.PackId}.pack"));
    }

    public string[] GetPathToPackDirectory()
    {
        return [Path.Combine(_receivePackRecoveryDirectory, "ReceivePack")];
    }
}
