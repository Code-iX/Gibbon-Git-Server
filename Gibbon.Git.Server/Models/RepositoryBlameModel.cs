namespace Gibbon.Git.Server.Models;

public class RepositoryBlameModel
{
    public string Name { get; set; }
    public string TreeName { get; set; }
    public string Path { get; set; }
    public RepositoryLogoDetailModel Logo { get; set; }
    public long FileSize { get; set; }
    public long LineCount { get; set; }
    public IEnumerable<RepositoryBlameHunkModel> Hunks { get; set; }
}