using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Models;

public class ForgotPasswordModel
{
    [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
    [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
    [Display(ResourceType = typeof(Resources), Name = "Home_ForgotPassword_Username")]
    public string Username { get; set; }
}