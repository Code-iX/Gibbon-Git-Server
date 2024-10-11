namespace Gibbon.Git.Server.Git.GitService.ReceivePackHook;

public record ParsedReceivePack(string PackId, string RepositoryName, IEnumerable<ReceivePackPktLine> PktLines, string PushedByUser, DateTime Timestamp, IEnumerable<ReceivePackCommit> Commits);
