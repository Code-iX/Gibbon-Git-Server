using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Models;

public class MePasswordModel
{
    public Guid Id { get; set; }

    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.Account_Edit_CurrentPassword))]
    [Required]
    public string OldPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(ResourceType = typeof(Resources), Name = "Account_Edit_NewPassword")]
    [Required]
    [StringLength(50, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_StringLength")]
    public string NewPassword { get; set; }

    [Compare("NewPassword", ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Compare")]
    [Display(ResourceType = typeof(Resources), Name = "Account_Edit_ConfirmPassword")]
    [Required]
    public string ConfirmPassword { get; set; }
}
