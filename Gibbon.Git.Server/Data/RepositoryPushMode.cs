using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Data;

public enum RepositoryPushMode
{
    [Display(ResourceType = typeof(Resources), Name = "No")]
    No = 0,
    [Display(ResourceType = typeof(Resources), Name = "Yes")]
    Yes,
    [Display(ResourceType = typeof(Resources), Name = "Global")]
    Global,
}