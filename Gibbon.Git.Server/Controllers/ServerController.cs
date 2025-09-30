using System.Threading.Tasks;

using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Git.GitDownloadService;
using Gibbon.Git.Server.Git.GitVersionService;
using Gibbon.Git.Server.Models;
using Gibbon.Git.Server.Security;
using Gibbon.Git.Server.Services;

using ICSharpCode.SharpZipLib.Zip;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace Gibbon.Git.Server.Controllers;

[Authorize(Roles = Roles.Admin)]
public class ServerController(IOptions<ApplicationSettings> options, IGitDownloadService downloadService, IServerSettingsService serverSettingsService, ICultureService cultureService, IOptions<GitSettings> gitOptions, IGitVersionService gitVersionService, IDatabaseHelperService databaseHelperService)
    : Controller
{
    private readonly IGitDownloadService _downloadService = downloadService;
    private readonly ApplicationSettings _applicationSettings = options.Value;
    private readonly GitSettings _gitSettings = gitOptions.Value;
    private readonly IServerSettingsService _serverSettingsService = serverSettingsService;
    private readonly ICultureService _cultureService = cultureService;
    private readonly IGitVersionService _gitVersionService = gitVersionService;
    private readonly IDatabaseHelperService _databaseHelperService = databaseHelperService;

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
    public async Task<IActionResult> Database()
    {
        var info = _databaseHelperService.GetDatabaseInformation();

        var model = new ServerDatabaseModel
        {
            DatabasePath = info.Path,
            DatabaseSize = info.Size
        };
        return View(model);
    }

    [Route("Server/Database/Download")]
    public async Task<IActionResult> Download()
    {
        var databaseFilePath = _databaseHelperService.GetDatabaseInformation().Path;

        var tempDatabasePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(databaseFilePath));

        System.IO.File.Copy(databaseFilePath, tempDatabasePath, true);

        var zipFileName = Path.ChangeExtension(Path.GetFileName(databaseFilePath), ".zip");

        using var memoryStream = new MemoryStream();
        var zipOutputStream = new ZipOutputStream(memoryStream);
        zipOutputStream.SetLevel(3);

        var entry = new ZipEntry(Path.GetFileName(tempDatabasePath))
        {
            DateTime = DateTime.Now,
            Size = new FileInfo(tempDatabasePath).Length
        };
        await zipOutputStream.PutNextEntryAsync(entry);

        await using (var fs = System.IO.File.OpenRead(tempDatabasePath))
        {
            await fs.CopyToAsync(zipOutputStream);
        }

        zipOutputStream.CloseEntry();
        zipOutputStream.IsStreamOwner = false;
        zipOutputStream.Finish();

        memoryStream.Position = 0;

        System.IO.File.Delete(tempDatabasePath);

        return File(memoryStream.ToArray(), "application/zip", zipFileName);
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

        var dateFormatItems = new List<SelectListItem>
        {
            new SelectListItem { Text = Resources.MeController_Settings_UseServerDefault, Value = "" },
            new SelectListItem { Text = "yyyy-MM-dd", Value = "yyyy-MM-dd" },
            new SelectListItem { Text = "dd.MM.yyyy", Value = "dd.MM.yyyy" },
            new SelectListItem { Text = "MM/dd/yyyy", Value = "MM/dd/yyyy" }
        };

        var timeFormatItems = new List<SelectListItem>
        {
            new SelectListItem { Text = Resources.MeController_Settings_UseServerDefault, Value = "" },
            new SelectListItem { Text = "HH:mm:ss", Value = "HH:mm:ss" },
            new SelectListItem { Text = "HH:mm", Value = "HH:mm" },
            new SelectListItem { Text = "hh:mm:ss tt", Value = "hh:mm:ss tt" },
            new SelectListItem { Text = "hh:mm tt", Value = "hh:mm tt" }
        };

        return View(new ServerSettingsModel
        {
            AllowAnonymousPush = settings.AllowAnonymousPush,
            AllowAnonymousRegistration = settings.AllowAnonymousRegistration,
            AllowUserRepositoryCreation = settings.AllowUserRepositoryCreation,
            DefaultLanguage = settings.DefaultLanguage,
            DefaultDateFormat = settings.DefaultDateFormat,
            DefaultTimeFormat = settings.DefaultTimeFormat,
            SiteTitle = settings.SiteTitle,
            SiteLogoUrl = settings.SiteLogoUrl,
            SiteCssUrl = settings.SiteCssUrl,
            IsCommitAuthorAvatarVisible = settings.IsCommitAuthorAvatarVisible,
            LinksRegex = settings.LinksRegex,
            LinksUrl = settings.LinksUrl,
            AvailableLanguages = cultureItems,
            AvailableDateFormats = dateFormatItems,
            AvailableTimeFormats = timeFormatItems
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
            var supportedCultures = await _cultureService.GetSupportedCultures();

            model.AvailableLanguages = supportedCultures
                .Select(cultureInfo => new SelectListItem
                {
                    Text = $"{cultureInfo.DisplayName} ({cultureInfo.Name})",
                    Value = cultureInfo.Name
                })
                .ToList();

            model.AvailableDateFormats = new List<SelectListItem>
            {
                new SelectListItem { Text = Resources.MeController_Settings_UseServerDefault, Value = "" },
                new SelectListItem { Text = "yyyy-MM-dd", Value = "yyyy-MM-dd" },
                new SelectListItem { Text = "dd.MM.yyyy", Value = "dd.MM.yyyy" },
                new SelectListItem { Text = "MM/dd/yyyy", Value = "MM/dd/yyyy" }
            };

            model.AvailableTimeFormats = new List<SelectListItem>
            {
                new SelectListItem { Text = Resources.MeController_Settings_UseServerDefault, Value = "" },
                new SelectListItem { Text = "HH:mm:ss", Value = "HH:mm:ss" },
                new SelectListItem { Text = "HH:mm", Value = "HH:mm" },
                new SelectListItem { Text = "hh:mm:ss tt", Value = "hh:mm:ss tt" },
                new SelectListItem { Text = "hh:mm tt", Value = "hh:mm tt" }
            };

            return View(model);
        }

        var settings = new ServerSettings
        {
            AllowAnonymousPush = model.AllowAnonymousPush,
            AllowAnonymousRegistration = model.AllowAnonymousRegistration,
            AllowUserRepositoryCreation = model.AllowUserRepositoryCreation,
            DefaultLanguage = model.DefaultLanguage,
            DefaultDateFormat = model.DefaultDateFormat,
            DefaultTimeFormat = model.DefaultTimeFormat,
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
