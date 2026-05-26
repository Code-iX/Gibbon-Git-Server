using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Gibbon.Git.Server.Models;

public class TeamEditModel
{
    public int Id { get; set; }

    [Remote("UniqueNameTeam", "Validation", AdditionalFields = "Id", ErrorMessage = "Validation_Duplicate_Name")]
    [Required(ErrorMessage = "Validation_Required")]
    [StringLength(50, ErrorMessage = "Validation_StringLength")]
    [Display(Name = "Team_Detail_Name")]
    public string Name { get; set; }

    [Display(Name = "Team_Detail_Description")]
    public string Description { get; set; }

    [Display(Name = "Team_Detail_Members")]
    public UserModel[] AllUsers { get; set; }

    public UserModel[] SelectedUsers { get; set; }

    public int[] PostedSelectedUsers { get; set; }
}
