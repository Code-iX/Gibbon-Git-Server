using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Models;

public class TeamDetailModel
{
    public int Id { get; set; }

    [Display(Name = "Team_Detail_Name")]
    public string Name { get; set; }

    [Display(Name = "Team_Detail_Description")]
    public string Description { get; set; }

    [Display(Name = "Team_Detail_Members")]
    public UserModel[] Members { get; set; }

    [Display(Name = "Team_Detail_Repositories")]
    public RepositoryModel[] Repositories { get; set; }
}
