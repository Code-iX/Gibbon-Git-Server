using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Models;

public class UserDetailModel
{
    public int Id { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Account_Detail_Username")]
    public string Username { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Account_Detail_Name")]
    public string Name { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Account_Detail_Surname")]
    public string Surname { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Account_Detail_Email")]
    public string Email { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Account_Detail_Roles")]
    public string[] Roles { get; set; }
}
