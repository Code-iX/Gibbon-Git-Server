using System.ComponentModel.DataAnnotations;
using Gibbon.Git.Server.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Gibbon.Git.Server.Models;

public class MeSettingsModel
{
    [Display(ResourceType = typeof(Resources), Name = "Settings_Global_DefaultLanguage")]
    public string PreferredLanguage { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Settings_NameFormat")]
    public NameFormat PreferredNameFormat { get; set; }

    /// <summary>
    /// This is the list of available languages for the user to choose from.
    /// </summary>
    /// <remarks>
    /// This is just for the user to choose from, why we don't need a display attribute.
    /// </remarks>
    internal List<SelectListItem> AvailableLanguages { get; set; }

    /// <summary>
    /// This is the list of available name formats for the user to choose from.
    /// </summary>
    internal List<SelectListItem> AvailableNameFormats { get; set; }
}
