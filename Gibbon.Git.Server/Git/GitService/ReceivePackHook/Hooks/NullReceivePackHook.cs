namespace Gibbon.Git.Server.Git.GitService.ReceivePackHook.Hooks;

public class NullReceivePackHook : IHookReceivePack
{
    public void PrePackReceive(ParsedReceivePack receivePack)
    {
        // do nothing
    }

    public void PostPackReceive(ParsedReceivePack receivePack, GitExecutionResult result)
    {
        // do nothing
    }
}