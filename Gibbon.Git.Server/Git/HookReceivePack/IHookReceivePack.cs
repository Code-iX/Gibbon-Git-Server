using Gibbon.Git.Server.Git.Models;

namespace Gibbon.Git.Server.Git.HookReceivePack;

/// <summary>
/// Implement this interface to receive notifications when a pack is recieved
/// and perform any relevant pre/post-processing operations.
/// </summary>
public interface IHookReceivePack
{
    void PrePackReceive(ParsedReceivePack receivePack);

    void PostPackReceive(ParsedReceivePack receivePack, GitExecutionResult result);
}
