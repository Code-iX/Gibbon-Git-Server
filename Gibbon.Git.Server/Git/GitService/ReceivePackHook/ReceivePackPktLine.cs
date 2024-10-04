namespace Gibbon.Git.Server.Git.GitService.ReceivePackHook;

public class ReceivePackPktLine
{
    public ReceivePackPktLine(string fromCommit, string toCommit, string refName)
    {
        FromCommit = fromCommit;
        ToCommit = toCommit;
        RefName = refName;
    }
    public string FromCommit { get; private set; }
    public string ToCommit { get; private set; }
    public string RefName { get; private set; }
}