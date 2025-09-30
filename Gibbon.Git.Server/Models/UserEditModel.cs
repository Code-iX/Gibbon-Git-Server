using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;

namespace Gibbon.Git.Server.Models;

public class UserEditModel
{
    public int Id { get; set; }

    [Remote("UniqueNameUser", "Validation", AdditionalFields = "Id", ErrorMessage = "Validation_Duplicate_Name")]
    [Required(ErrorMessage = "Validation_Required")]
    [Display(Name = "Account_Edit_Username")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Validation_Required")]
    [Display(Name = "Account_Edit_Name")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Validation_Required")]
    [Display(Name = "Account_Edit_Surname")]
    public string Surname { get; set; }

    [Required(ErrorMessage = "Validation_Required")]
    [EmailAddress(ErrorMessage = "Validation_Email")]
    [DataType(DataType.EmailAddress)]
    [Display(Name = "Account_Edit_Email")]
    public string Email { get; set; }

    [Display(Name = "Account_Edit_Roles")]
    public string[] Roles { get; set; } = [];

    public string[] SelectedRoles { get; set; } = [];
    public string[] PostedSelectedRoles { get; set; } = [];
}
