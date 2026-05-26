using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Data;

public enum RepositoryPushMode
{
    [Display(Name = "No")]
    No = 0,
    [Display(Name = "Yes")]
    Yes,
    [Display(Name = "Global")]
    Global,
}