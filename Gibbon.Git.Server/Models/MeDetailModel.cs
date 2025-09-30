using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Models;

public class MeDetailModel
{
    [Display(Name = "Account_Detail_Username")]
    public string Username { get; set; }

    [Display(Name = "Account_Detail_Name")]
    public string Name { get; set; }

    [Display(Name = "Account_Detail_Surname")]
    public string Surname { get; set; }

    [Display(Name = "Account_Detail_Email")]
    public string Email { get; set; }

    [Display(Name = "Account_Detail_Roles")]
    public string[] Roles { get; set; }
}
