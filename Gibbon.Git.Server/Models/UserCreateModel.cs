using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Gibbon.Git.Server.Models;

public class UserCreateModel
{
    [Remote("UniqueNameUser", "Validation", AdditionalFields = "Id", ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Duplicate_Name")]
    [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
    [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
    [Display(ResourceType = typeof(Resources), Name = "Account_Create_Username")]
    public string Username { get; set; }

    [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
    [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
    [Display(ResourceType = typeof(Resources), Name = "Account_Create_Name")]
    public string Name { get; set; }

    [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
    [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
    [Display(ResourceType = typeof(Resources), Name = "Account_Create_Surname")]
    public string Surname { get; set; }

    [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
    [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
    [EmailAddress(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Email")]
    [DataType(DataType.EmailAddress)]
    [Display(ResourceType = typeof(Resources), Name = "Account_Create_Email")]
    public string Email { get; set; }

    [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
    [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(Resources), Name = "Account_Create_Password")]
    public string Password { get; set; }

    [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
    [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
    [Compare("Password", ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Compare")]
    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(Resources), Name = "Account_Create_ConfirmPassword")]
    public string ConfirmPassword { get; set; }
}
