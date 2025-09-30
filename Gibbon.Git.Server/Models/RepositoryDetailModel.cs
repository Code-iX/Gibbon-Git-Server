using System.ComponentModel.DataAnnotations;
using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Middleware.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Gibbon.Git.Server.Models;

public class RepositoryDetailModel
{
    public int Id { get; set; }

    [Remote("UniqueNameRepo", "Validation", AdditionalFields = "Id", ErrorMessage = "Validation_Duplicate_Name")]
    [Required(ErrorMessage = "Validation_Required")]
    [RepositoryName(ErrorMessage = "Validation_FileName")]
    [StringLength(50, ErrorMessage = "Validation_StringLength")]
    [Display(Name = "Repository_Detail_Name")]
    public string Name { get; set; } = "";

    [Display(Name = "Repository_Detail_Group")]
    [StringLength(255, ErrorMessage = "Validation_StringLength")]
    public string Group { get; set; }

    [Display(Name = "Repository_Detail_Description")]
    [StringLength(255, ErrorMessage = "Validation_StringLength")]
    public string Description { get; set; }

    [Display(Name = "Repository_Detail_Users")]
    public UserModel[] Users { get; set; } = [];
    public int[] PostedSelectedUsers { get; set; } = [];
    public UserModel[] AllUsers { get; set; } = [];

    [Display(Name = "Repository_Detail_Teams")]
    public TeamModel[] Teams { get; set; } = [];
    public int[] PostedSelectedTeams { get; set; } = [];
    public TeamModel[] AllTeams { get; set; } = [];

    [Display(Name = "Repository_Detail_Administrators")]
    public UserModel[] Administrators { get; set; } = [];
    public int[] PostedSelectedAdministrators { get; set; } = [];
    public UserModel[] AllAdministrators { get; set; } = [];

    [Display(Name = "Repository_Detail_IsCurrentUserAdmin")]
    public bool IsCurrentUserAdministrator { get; set; }

    [Display(Name = "Repository_Detail_Anonymous")]
    public bool AllowAnonymous { get; set; }

    [EnumDataType(typeof(RepositoryPushMode), ErrorMessage = "Repository_Edit_InvalidAnonymousPushMode")]
    [Display(Name = "Repository_Detail_AllowAnonymousPush")]
    public RepositoryPushMode AllowAnonymousPush { get; set; } = RepositoryPushMode.Global;

    [Display(Name = "Repository_Detail_Status")]
    public RepositoryDetailStatus Status { get; set; }

    public RepositoryLogoDetailModel Logo { get; set; }
    public string GitUrl { get; set; }
    public string PersonalGitUrl { get; set; }

    [Remote("IsValidRegex", "Validation")]
    [IsValidRegex]
    [Display(Name = "Settings_Global_LinksRegex")]
    public string LinksRegex { get; set; }
    [Display(Name = "Settings_Global_LinksUrl")]
    public string LinksUrl { get; set; }
    [Display(Name = "Repository_Detail_LinksUseGlobal")]
    public bool LinksUseGlobal { get; set; } = true;
}
