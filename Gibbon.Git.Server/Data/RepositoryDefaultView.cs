using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Data;

public enum RepositoryDefaultView
{
    [Display(ResourceType = typeof(Resources), Name = "Repository_Detail_Detail")]
    Detail = 0,
    [Display(ResourceType = typeof(Resources), Name = "Repository_Layout_Browse")]
    Tree,
    [Display(ResourceType = typeof(Resources), Name = "Repository_Layout_Commits")]
    Commits,
    [Display(ResourceType = typeof(Resources), Name = "Repository_Layout_Tags")]
    Tags,
}
