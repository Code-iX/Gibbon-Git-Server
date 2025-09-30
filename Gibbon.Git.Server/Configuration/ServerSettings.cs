namespace Gibbon.Git.Server.Configuration;

public sealed class ServerSettings
{
    public bool AllowAnonymousPush { get; set; }
    public bool AllowUserRepositoryCreation { get; set; }
    public bool AllowAnonymousRegistration { get; set; }
    public string DefaultLanguage { get; set; }
    public string DefaultDateFormat { get; set; }
    public string DefaultTimeFormat { get; set; }
    public string SiteTitle { get; set; }
    public string SiteLogoUrl { get; set; }
    public string SiteCssUrl { get; set; }
    public bool IsCommitAuthorAvatarVisible { get; set; }
    public string LinksRegex { get; set; }
    public string LinksUrl { get; set; }

    public bool HasCustomSiteLogo() => !string.IsNullOrWhiteSpace(SiteLogoUrl);

    public bool HasCustomSiteCss() => !string.IsNullOrWhiteSpace(SiteCssUrl);

    public bool HasLinks() => !string.IsNullOrWhiteSpace(LinksRegex);

    public string GetSiteTitle() => !string.IsNullOrWhiteSpace(SiteTitle) ? SiteTitle : Resources.Product_Name;

}
