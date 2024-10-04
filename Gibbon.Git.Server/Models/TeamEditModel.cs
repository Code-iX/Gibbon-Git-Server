using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Gibbon.Git.Server.Models;

public class TeamEditModel
{
    public Guid Id { get; set; }

    [Remote("UniqueNameTeam", "Validation", AdditionalFields = "Id", ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Duplicate_Name")]
    [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
    [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
    [Display(ResourceType = typeof(Resources), Name = "Team_Detail_Name")]
    public string Name { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Team_Detail_Description")]
    public string Description { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Team_Detail_Members")]
    public UserModel[] AllUsers { get; set; }

    public UserModel[] SelectedUsers { get; set; }

    public Guid[] PostedSelectedUsers { get; set; }
}