namespace Gibbon.Git.Server.Git.GitService.ReceivePackHook;

public record ReceivePackCommit(string Id, string Tree, IEnumerable<string> Parents, ReceivePackCommitSignature Author, ReceivePackCommitSignature Committer, string Message);
