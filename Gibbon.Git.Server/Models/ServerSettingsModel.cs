using System.ComponentModel.DataAnnotations;

using Gibbon.Git.Server.Helpers;
using Gibbon.Git.Server.Middleware.Attributes;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Gibbon.Git.Server.Models;

public class ServerSettingsModel
{
    [Display(ResourceType = typeof(Resources), Name = "Settings_Global_AllowAnonymousPush")]
    public bool AllowAnonymousPush { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Settings_Global_AllowAnonymousRegistration")]
    public bool AllowAnonymousRegistration { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Settings_Global_AllowUserRepositoryCreation")]
    public bool AllowUserRepositoryCreation { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Settings_Global_AllowPushToCreate")]
    public bool AllowPushToCreate { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Settings_Global_DefaultLanguage")]
    public string DefaultLanguage { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Settings_Global_SiteTitle")]
    public string SiteTitle { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Settings_Global_SiteLogoUrl")]
    public string SiteLogoUrl { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Settings_Global_SiteCssUrl")]
    public string SiteCssUrl { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Settings_Global_IsCommitAuthorAvatarVisible")]
    public bool IsCommitAuthorAvatarVisible { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Settings_Global_LinksUrl")]
    public string LinksUrl { get; set; }

    [Remote("IsValidRegex", "Validation")]
    [IsValidRegex]
    [Display(ResourceType = typeof(Resources), Name = "Settings_Global_LinksRegex")]
    public string LinksRegex { get; set; }

    public List<SelectListItem> AvailableLanguages { get; set; }
}

public class ServerOverviewModel
{
    [Display(ResourceType = typeof(Resources), Name = "ServerOverviewModel_DotNetVersion")]
    public string DotNetVersion { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "ServerOverviewModel_IsDemoActive")]
    public bool IsDemoActive { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "ServerOverviewModel_ApplicationPath")]
    public string ApplicationPath { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "ServerOverviewModel_DataPath")]
    public string DataPath { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "ServerOverviewModel_RepositoryPath")]
    public string RepositoryPath { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "ServerOverviewModel_GitPath")]
    public string GitPath { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "ServerOverviewModel_GitHomePath")]
    public string GitHomePath { get; set; }

    [Display(Name = "Application Version")]
    public string ApplicationVersion { get; set; } = VersionHelper.GetAssemblyVersion();
}

public class ServerGitModel
{
    [Display(Name = "Git installed")]
    public bool IsGitAvailable { get; set; }
}
