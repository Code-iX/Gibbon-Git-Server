namespace Gibbon.Git.Server.Git.Models;

public record ParsedReceivePack(string PackId, string RepositoryName, IEnumerable<ReceivePackPktLine> PktLines, string PushedByUser, DateTime Timestamp, IEnumerable<ReceivePackCommit> Commits);
