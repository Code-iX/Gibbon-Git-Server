namespace Gibbon.Git.Server.Git.GitService.ReceivePackHook;

public record ReceivePackCommitSignature(string Name, string Email, DateTimeOffset Timestamp);
