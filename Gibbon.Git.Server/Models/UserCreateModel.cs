using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Gibbon.Git.Server.Models;

public class UserCreateModel
{
    [Remote("UniqueNameUser", "Validation", AdditionalFields = "Id", ErrorMessage = "Validation_Duplicate_Name")]
    [Required(ErrorMessage = "Validation_Required")]
    [StringLength(50, ErrorMessage = "Validation_StringLength")]
    [Display(Name = "Account_Create_Username")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Validation_Required")]
    [StringLength(50, ErrorMessage = "Validation_StringLength")]
    [Display(Name = "Account_Create_Name")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Validation_Required")]
    [StringLength(50, ErrorMessage = "Validation_StringLength")]
    [Display(Name = "Account_Create_Surname")]
    public string Surname { get; set; }

    [Required(ErrorMessage = "Validation_Required")]
    [StringLength(50, ErrorMessage = "Validation_StringLength")]
    [EmailAddress(ErrorMessage = "Validation_Email")]
    [DataType(DataType.EmailAddress)]
    [Display(Name = "Account_Create_Email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Validation_Required")]
    [StringLength(50, ErrorMessage = "Validation_StringLength")]
    [DataType(DataType.Password)]
    [Display(Name = "Account_Create_Password")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Validation_Required")]
    [StringLength(50, ErrorMessage = "Validation_StringLength")]
    [Compare("Password", ErrorMessage = "Validation_Compare")]
    [DataType(DataType.Password)]
    [Display(Name = "Account_Create_ConfirmPassword")]
    public string ConfirmPassword { get; set; }
}
