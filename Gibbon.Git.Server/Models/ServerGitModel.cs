using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Models;

public class ServerGitModel
{
    [Display(Name = "ServerGitModel_IsGitAvailable")]
    public bool IsGitAvailable { get; set; }
}
