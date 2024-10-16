using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Models;

public class MeEditModel
{
    public string Username { get; set; }

    [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
    [Display(ResourceType = typeof(Resources), Name = "Account_Edit_Name")]
    public string Name { get; set; }

    [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
    [Display(ResourceType = typeof(Resources), Name = "Account_Edit_Surname")]
    public string Surname { get; set; }

    [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Required")]
    [EmailAddress(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Validation_Email")]
    [DataType(DataType.EmailAddress)]
    [Display(ResourceType = typeof(Resources), Name = "Account_Edit_Email")]
    public string Email { get; set; }
}
