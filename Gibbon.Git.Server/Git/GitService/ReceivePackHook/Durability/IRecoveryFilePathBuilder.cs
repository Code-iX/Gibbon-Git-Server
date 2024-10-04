namespace Gibbon.Git.Server.Git.GitService.ReceivePackHook.Durability;

public interface IRecoveryFilePathBuilder
{
    string GetPathToResultFile(string correlationId, string repositoryName, string serviceName);

    string GetPathToPackFile(ParsedReceivePack receivePack);

    string[] GetPathToPackDirectory();
}