using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;

namespace Gibbon.Git.Server.Models;

public class UserEditModel
{
    public int Id { get; set; }

    [Remote("UniqueNameUser", "Validation", AdditionalFields = "Id", ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Duplicate_Name")]
    [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
    [Display(ResourceType = typeof(Resources), Name = "Account_Edit_Username")]
    public string Username { get; set; }

    [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
    [Display(ResourceType = typeof(Resources), Name = "Account_Edit_Name")]
    public string Name { get; set; }

    [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
    [Display(ResourceType = typeof(Resources), Name = "Account_Edit_Surname")]
    public string Surname { get; set; }

    [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
    [EmailAddress(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Email")]
    [DataType(DataType.EmailAddress)]
    [Display(ResourceType = typeof(Resources), Name = "Account_Edit_Email")]
    public string Email { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Account_Edit_Roles")]
    public string[] Roles { get; set; } = [];

    public string[] SelectedRoles { get; set; } = [];
    public string[] PostedSelectedRoles { get; set; } = [];
}
