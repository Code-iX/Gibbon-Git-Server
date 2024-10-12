namespace Gibbon.Git.Server.Git.Models;

public record ReceivePackPktLine(string FromCommit, string ToCommit, string RefName);
