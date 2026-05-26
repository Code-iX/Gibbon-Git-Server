using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Models;

public class MePasswordModel
{
    public Guid Id { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Account_Edit_CurrentPassword")]
    [Required]
    public string OldPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Account_Edit_NewPassword")]
    [Required]
    [StringLength(50, ErrorMessage = "Validation_StringLength")]
    public string NewPassword { get; set; }

    [Compare("NewPassword", ErrorMessage = "Validation_Compare")]
    [Display(Name = "Account_Edit_ConfirmPassword")]
    [Required]
    public string ConfirmPassword { get; set; }
}
