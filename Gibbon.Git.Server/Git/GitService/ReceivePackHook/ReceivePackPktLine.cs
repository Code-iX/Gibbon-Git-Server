namespace Gibbon.Git.Server.Git.GitService.ReceivePackHook;

public record ReceivePackPktLine(string FromCommit, string ToCommit, string RefName);
