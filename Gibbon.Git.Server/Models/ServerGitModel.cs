using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Models;

public class ServerGitModel
{
    [Display(ResourceType = typeof(Resources), Name = "ServerGitModel_IsGitAvailable")]
    public bool IsGitAvailable { get; set; }
}
