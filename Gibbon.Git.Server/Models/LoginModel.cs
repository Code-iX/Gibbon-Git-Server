using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Models;

public class LoginModel
{
    [Required(ErrorMessage = "Validation_Required")]
    [StringLength(50, ErrorMessage = "Validation_StringLength")]
    [Display(Name = "Home_Login_Username")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Validation_Required")]
    [StringLength(50, ErrorMessage = "Validation_StringLength")]
    [DataType(DataType.Password)]
    [Display(Name = "Home_Login_Password")]
    public string Password { get; set; }

    [Display(Name = "Home_Login_RememberMe")]
    public bool RememberMe { get; set; }

    public int DatabaseResetCode { get; set; }

    public string ReturnUrl { get; set; }
}