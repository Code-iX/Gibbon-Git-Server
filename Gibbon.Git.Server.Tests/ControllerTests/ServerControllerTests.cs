using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Controllers;
using Gibbon.Git.Server.Git;
using Gibbon.Git.Server.Git.GitDownloadService;
using Gibbon.Git.Server.Models;
using Gibbon.Git.Server.Services;
using Gibbon.Git.Server.Tests.TestHelper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NSubstitute;

namespace Gibbon.Git.Server.Tests.ControllerTests;

[TestClass]
public class ServerControllerTests : ControllerTestBase<ServerController>
{
    private IGitDownloadService _downloadService = null!;
    private IServerSettingsService _serverSettingsService = null!;
    private ICultureService _cultureService = null!;
    private ApplicationSettings _applicationSettings = null!;

    protected override void ConfigureServices(ServiceCollection services)
    {
        _applicationSettings = new ApplicationSettings { DemoModeActive = false };
        services.AddSubstitute<IOptions<ApplicationSettings>>(options =>
        {
            options.Value.Returns(_applicationSettings);
        });
        var versionService = services.AddSubstitute<IGitVersionService>();
        versionService.IsGitAvailable.Returns(true);
        _downloadService = services.AddSubstitute<IGitDownloadService>();
        _serverSettingsService = services.AddSubstitute<IServerSettingsService>();
        _cultureService = services.AddSubstitute<ICultureService>();
        services.AddSubstitute<IDatabaseHelperService>();
    }

    [TestMethod]
    public void Index_RedirectsToSettings()
    {
        // Act
        var result = Controller.Index() as ViewResult;

        // Assert
        Assert.IsNotNull(result);

        var model = result.Model as ServerOverviewModel;
        Assert.IsNotNull(model);
    }

    [TestMethod]
    public async Task Settings_ReturnsViewWithCorrectModel()
    {
        // Arrange
        List<CultureInfo> cultures =
        [
            new("en"),
            new("de")
        ];
        _cultureService.GetSupportedCultures().Returns(cultures);

        var serverSettings = new ServerSettings
        {
            AllowAnonymousPush = true,
            AllowAnonymousRegistration = true,
            AllowUserRepositoryCreation = true,
            AllowPushToCreate = true,
            DefaultLanguage = "en",
            SiteTitle = "Test Site",
            SiteLogoUrl = "logo.png",
            SiteCssUrl = "styles.css",
            IsCommitAuthorAvatarVisible = true,
            LinksRegex = "regex",
            LinksUrl = "url"
        };
        _serverSettingsService.GetSettings().Returns(serverSettings);

        // Act
        var result = await Controller.Settings() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        var model = result.Model as ServerSettingsModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(serverSettings.AllowAnonymousPush, model.AllowAnonymousPush);
        Assert.AreEqual(serverSettings.DefaultLanguage, model.DefaultLanguage);
        Assert.AreEqual(2, model.AvailableLanguages.Count);
    }

    [TestMethod]
    public void DownloadGit_RedirectsToSettings()
    {
        // Act
        var result = Controller.DownloadGit() as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Settings", result.ActionName);
        _downloadService.Received(1).EnsureDownloadedAsync();
    }

    [TestMethod]
    public async Task Settings_Post_ReturnsUnauthorizedIfDemoModeActive()
    {
        // Arrange
        _applicationSettings.DemoModeActive = true;
        var model = new ServerSettingsModel();

        // Act
        var result = await Controller.Settings(model) as UnauthorizedResult;

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task Settings_Post_RedirectsToSettingsOnSuccess()
    {
        // Arrange
        var model = new ServerSettingsModel { SiteTitle = "New Title" };

        // Act
        var result = await Controller.Settings(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Settings", result.ActionName);
        await _serverSettingsService.Received(1).SaveSettings(Arg.Any<ServerSettings>());
    }

    [TestMethod]
    public void ResetSettings_RedirectsToSettings()
    {
        // Act
        var result = Controller.ResetSettings() as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Settings", result.ActionName);
        _serverSettingsService.Received(1).ResetSettings();
    }
}
