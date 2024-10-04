using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Gibbon.Git.Server.Models;

public class UserSettingsModel
{
    [Display(ResourceType = typeof(Resources), Name = "Settings_Global_DefaultLanguage")]
    public string DefaultLanguage { get; set; }

    public List<SelectListItem> AvailableLanguages { get; set; }
}
