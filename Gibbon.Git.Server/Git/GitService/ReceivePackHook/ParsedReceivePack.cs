namespace Gibbon.Git.Server.Git.GitService.ReceivePackHook;

public class ParsedReceivePack
{
    public ParsedReceivePack (string packId, string repositoryName, IEnumerable<ReceivePackPktLine> pktLines, string pushedByUser, DateTime timestamp, IEnumerable<ReceivePackCommit> commits)
    {
        PackId = packId;
        PktLines = pktLines;
        PushedByUser = pushedByUser;
        Timestamp = timestamp;
        RepositoryName = repositoryName;
        Commits = commits;
    }

    public string PackId { get; private set; }

    public IEnumerable<ReceivePackPktLine> PktLines { get; private set; }

    public IEnumerable<ReceivePackCommit> Commits { get; private set; }

    public string PushedByUser { get; private set; }

    public DateTime Timestamp { get; private set; }

    public string RepositoryName { get; private set; }
}