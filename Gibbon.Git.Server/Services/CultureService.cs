using System.Globalization;
using System.Threading.Tasks;

using Gibbon.Git.Server.Configuration;

using Microsoft.AspNetCore.Hosting;

namespace Gibbon.Git.Server.Services;

internal sealed class CultureService(ServerSettings serverSettings, IUserSettingsService userSettingsService, IWebHostEnvironment webHostEnvironment)
    : ICultureService
{
    private readonly ServerSettings _serverSettings = serverSettings;
    private readonly IUserSettingsService _userSettingsService = userSettingsService;
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

    public async Task<CultureInfo> GetSelectedCultureInfo(Guid userId)
    {
        var userSettings = userId != Guid.Empty ? await _userSettingsService.GetSettings(userId) : _userSettingsService.GetDefaultSettings();

        var language = userSettings.PreferredLanguage ?? _serverSettings.DefaultLanguage;

        return new CultureInfo(language);
    }

    public async Task<List<CultureInfo>> GetSupportedCultures()
    {
        var resourceFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "App_Resources");

        return Directory.GetFiles(resourceFilePath, "Resources.*.resx")
            .Select(f => Path.GetFileNameWithoutExtension(f).Split('.')[1])
            .Append("en")
            .Select(CultureInfo.GetCultureInfo)
            .OrderBy(i => i.Name)
            .ToList();
    }
}
