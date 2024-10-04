using System.Threading.Tasks;

using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Git;
using Gibbon.Git.Server.Git.GitDownloadService;
using Gibbon.Git.Server.Models;
using Gibbon.Git.Server.Security;
using Gibbon.Git.Server.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace Gibbon.Git.Server.Controllers;

[Authorize(Roles = Definitions.Roles.Administrator)]
public class ServerController(IOptions<ApplicationSettings> options, IGitDownloadService downloadService, IServerSettingsService serverSettingsService, ICultureService cultureService, IOptions<GitSettings> gitOptions, IGitVersionService gitVersionService)
    : Controller
{
    private readonly IGitDownloadService _downloadService = downloadService;
    private readonly ApplicationSettings _applicationSettings = options.Value;
    private readonly GitSettings _gitSettings = gitOptions.Value;
    private readonly IServerSettingsService _serverSettingsService = serverSettingsService;
    private readonly ICultureService _cultureService = cultureService;
    private readonly IGitVersionService _gitVersionService = gitVersionService;

    [HttpGet]
    public IActionResult Index()
    {
        var model = new ServerOverviewModel
        {
            IsDemoActive = _applicationSettings.DemoModeActive,
            DotNetVersion = Environment.Version.ToString(),
            ApplicationPath = Path.GetDirectoryName(Environment.CommandLine),
            DataPath = _applicationSettings.DataPath,
            RepositoryPath = _applicationSettings.RepositoryPath,
            GitPath = _gitSettings.BinaryPath,
            GitHomePath = _gitSettings.HomePath
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Git()
    {
        ServerGitModel model = new ServerGitModel
        {
            IsGitAvailable = _gitVersionService.IsGitAvailable
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var cultures = await _cultureService.GetSupportedCultures();

        var settings = _serverSettingsService.GetSettings();
        var cultureItems = cultures
            .Select(cultureInfo => new SelectListItem
            {
                Text = $"{cultureInfo.Name} - {cultureInfo.DisplayName}",
                Value = cultureInfo.Name,
                Selected = cultureInfo.Name == settings.DefaultLanguage
            })
            .ToList();

        return View(new ServerSettingsModel
        {
            AllowAnonymousPush = settings.AllowAnonymousPush,
            AllowAnonymousRegistration = settings.AllowAnonymousRegistration,
            AllowUserRepositoryCreation = settings.AllowUserRepositoryCreation,
            AllowPushToCreate = settings.AllowPushToCreate,
            DefaultLanguage = settings.DefaultLanguage,
            SiteTitle = settings.SiteTitle,
            SiteLogoUrl = settings.SiteLogoUrl,
            SiteCssUrl = settings.SiteCssUrl,
            IsCommitAuthorAvatarVisible = settings.IsCommitAuthorAvatarVisible,
            LinksRegex = settings.LinksRegex,
            LinksUrl = settings.LinksUrl,
            AvailableLanguages = cultureItems
        });
    }

    public IActionResult DownloadGit()
    {
        _downloadService.EnsureDownloadedAsync();

        return RedirectToAction("Settings");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(ServerSettingsModel model)
    {
        if (_applicationSettings.DemoModeActive)
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var settings = new ServerSettings
        {
            AllowAnonymousPush = model.AllowAnonymousPush,
            AllowAnonymousRegistration = model.AllowAnonymousRegistration,
            AllowUserRepositoryCreation = model.AllowUserRepositoryCreation,
            AllowPushToCreate = model.AllowPushToCreate,
            DefaultLanguage = model.DefaultLanguage,
            SiteTitle = model.SiteTitle,
            SiteLogoUrl = model.SiteLogoUrl,
            SiteCssUrl = model.SiteCssUrl,
            IsCommitAuthorAvatarVisible = model.IsCommitAuthorAvatarVisible,
            LinksRegex = model.LinksRegex,
            LinksUrl = model.LinksUrl
        };
        await _serverSettingsService.SaveSettings(settings);
        TempData["UpdateSuccess"] = true;

        return RedirectToAction("Settings");

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ResetSettings()
    {
        if (ModelState.IsValid)
        {
            _serverSettingsService.ResetSettings();
            TempData["ResetSuccess"] = true;
        }

        return RedirectToAction("Settings");
    }
}
