using Gibbon.Git.Server.Data;

namespace Gibbon.Git.Server.Helpers;

public static class RepositoryViewHelper
{
    public static string GetActionName(RepositoryDefaultView view)
    {
        return view switch
        {
            RepositoryDefaultView.Detail => "Detail",
            RepositoryDefaultView.Tree => "Tree",
            RepositoryDefaultView.Commits => "Commits",
            RepositoryDefaultView.Tags => "Tags",
            _ => "Detail"
        };
    }
}
