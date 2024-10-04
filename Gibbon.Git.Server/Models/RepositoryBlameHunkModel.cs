namespace Gibbon.Git.Server.Models;

public class RepositoryBlameHunkModel
{
    public RepositoryCommitModel Commit { get; set; }
    public string[] Lines { get; set; }
}