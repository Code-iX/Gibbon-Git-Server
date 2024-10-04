namespace Gibbon.Git.Server.Git.GitService.ReceivePackHook;

public class ReceivePackCommitSignature
{
    public ReceivePackCommitSignature(string name, string email, DateTimeOffset timestamp)
    {
        Name = name;
        Email = email;
        Timestamp = timestamp;
    }

    public string Name { get; private set; }

    public string Email { get; private set; }

    public DateTimeOffset Timestamp { get; private set; }
}