namespace Gibbon.Git.Server.Git.GitService.ReceivePackHook.Durability;

/// <summary>
/// Ensures directories for generated recovery paths exist
/// </summary>
public class AutoCreateMissingRecoveryDirectories(IRecoveryFilePathBuilder pathBuilder) : IRecoveryFilePathBuilder
{
    private readonly IRecoveryFilePathBuilder _pathBuilder = pathBuilder;

    public string GetPathToResultFile(string correlationId, string repositoryName, string serviceName)
    {
        return CreateDirectoryForFile(_pathBuilder.GetPathToResultFile(correlationId, repositoryName, serviceName));
    }

    public string GetPathToPackFile(ParsedReceivePack receivePack)
    {
        return CreateDirectoryForFile(_pathBuilder.GetPathToPackFile(receivePack));
    }

    public string[] GetPathToPackDirectory()
    {
        var dirs = _pathBuilder.GetPathToPackDirectory();
        foreach(var dir in dirs)
        {
            Directory.CreateDirectory(dir);
        }
        return dirs;
    }

    private string CreateDirectoryForFile(string filePath)
    {
        var dirPath = Path.GetDirectoryName(filePath);
        Directory.CreateDirectory(dirPath);
        return filePath;
    }
}
