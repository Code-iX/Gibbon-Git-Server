namespace Gibbon.Git.Server.Models;

public class RepositoryCommitsModel
{
    public string Name { get; set; }
    public RepositoryLogoDetailModel Logo { get; set; }
    public IEnumerable<RepositoryCommitModel> Commits { get; set; }
    public PaginationModel Pagination { get; set; }
}