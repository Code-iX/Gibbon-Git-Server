using LibGit2Sharp;

namespace Gibbon.Git.Server.Models;

public class RepositoryCommitChangeModel
{
    public string ChangeId { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public ChangeKind Status { get; set; }
    public int LinesAdded { get; set; }
    public int LinesDeleted { get; set; }
    public string Patch { get; set; }
    public int LinesChanged => LinesAdded + LinesDeleted;
}