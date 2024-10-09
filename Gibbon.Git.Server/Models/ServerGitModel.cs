using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Models;

public class ServerGitModel
{
    [Display(Name = "Git installed")]
    public bool IsGitAvailable { get; set; }
}