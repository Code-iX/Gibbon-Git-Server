using System.ComponentModel.DataAnnotations;
using Gibbon.Git.Server.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Gibbon.Git.Server.Models;

public class MeSettingsModel
{
    [Display(ResourceType = typeof(Resources), Name = "Settings_Global_DefaultLanguage")]
    public string PreferredLanguage { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Settings_User_DefaultRepositoryView")]
    public RepositoryDefaultView? DefaultRepositoryView { get; set; }

    /// <summary>
    /// This is the list of available languages for the user to choose from.
    /// </summary>
    /// <remarks>
    /// This is just for the user to choose from, why we don't need a display attribute.
    /// </remarks>
    internal List<SelectListItem> AvailableLanguages { get; set; }
}
