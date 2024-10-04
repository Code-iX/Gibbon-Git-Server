namespace Gibbon.Git.Server.Git.GitService.ReceivePackHook;

public class ReceivePackCommit
{
    public ReceivePackCommit(string id, string tree, IEnumerable<string> parents, 
        ReceivePackCommitSignature author, ReceivePackCommitSignature committer, string message)
    {
        Id = id;
        Tree = tree;
        Parents = parents;
        Author = author;
        Committer = committer;
        Message = message;
    }

    public string Id { get; private set; }
    public string Tree { get; private set; }
    public IEnumerable<string> Parents { get; private set; }
    public ReceivePackCommitSignature Author { get; private set; }
    public ReceivePackCommitSignature Committer { get; private set; }
    public string Message { get; private set; }
}