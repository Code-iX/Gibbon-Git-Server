namespace Gibbon.Git.Server.Git.Models;

public record ReceivePackCommit(string Id, string Tree, IEnumerable<string> Parents, ReceivePackCommitSignature Author, ReceivePackCommitSignature Committer, string Message);
