namespace Gibbon.Git.Server.Git.GitService.ReceivePackHook.Durability;

/// <summary>
/// Perhaps there's a better way to handle wiring up simple types in Unity but i haven't found it
/// </summary>
public class NamedArguments
{
    public record FailedPackWaitTimeBeforeExecution(TimeSpan Value);

    public record ReceivePackRecoveryDirectory(string Value);
}
