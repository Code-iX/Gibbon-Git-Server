using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Gibbon.Git.Server.Models;

public class ResetPasswordModel
{
    [Required(ErrorMessage = "Validation_Required")]
    [StringLength(50, ErrorMessage = "Validation_StringLength")]
    [Display(Name = "Home_ResetPassword_Username")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Validation_Required")]
    [StringLength(50, ErrorMessage = "Validation_StringLength")]
    [DataType(DataType.Password)]
    [Display(Name = "Home_ResetPassword_Password")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Validation_Required")]
    [StringLength(50, ErrorMessage = "Validation_StringLength")]
    [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Validation_Compare")]
    [DataType(DataType.Password)]
    [Display(Name = "Home_ResetPassword_ConfirmPassword")]
    public string ConfirmPassword { get; set; }

    [HiddenInput]
    public string Digest { get; set; }
}