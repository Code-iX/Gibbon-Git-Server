using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Models;

public class ForgotPasswordModel
{
    [Required(ErrorMessage = "Validation_Required")]
    [StringLength(50, ErrorMessage = "Validation_StringLength")]
    [Display(Name = "Home_ForgotPassword_Username")]
    public string Username { get; set; }
}