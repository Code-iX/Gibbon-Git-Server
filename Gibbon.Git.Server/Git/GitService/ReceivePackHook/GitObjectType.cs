namespace Gibbon.Git.Server.Git.GitService.ReceivePackHook;

public enum GitObjectType
{
    Unknown = 0,
    Commit = 1,
    Tree = 2,
    Blob = 3,
    Tag = 4,
    OfsDelta = 6,
    RefDelta = 7
}
