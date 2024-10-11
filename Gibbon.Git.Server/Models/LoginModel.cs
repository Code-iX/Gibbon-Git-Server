using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Models;

public class LoginModel
{
    [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
    [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
    [Display(ResourceType = typeof(Resources), Name = "Home_Login_Username")]
    public string Username { get; set; }

    [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
    [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(Resources), Name = "Home_Login_Password")]
    public string Password { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Home_Login_RememberMe")]
    public bool RememberMe { get; set; }

    public int DatabaseResetCode { get; set; }

    public string ReturnUrl { get; set; }
}