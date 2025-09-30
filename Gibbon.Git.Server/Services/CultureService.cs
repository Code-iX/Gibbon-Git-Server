using System.Globalization;
using System.Threading.Tasks;

using Gibbon.Git.Server.Configuration;

using Microsoft.AspNetCore.Hosting;

namespace Gibbon.Git.Server.Services;

internal sealed class CultureService(ServerSettings serverSettings, IUserSettingsService userSettingsService, IWebHostEnvironment webHostEnvironment)
    : ICultureService
{
    private const string DefaultLanguage = "en-US";

    private readonly ServerSettings _serverSettings = serverSettings;
    private readonly IUserSettingsService _userSettingsService = userSettingsService;
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

    public async Task<CultureInfo> GetSelectedCultureInfo(int userId)
    {
        var userSettings = userId != 0 ? await _userSettingsService.GetSettings(userId) : _userSettingsService.GetDefaultSettings();

        var language = userSettings.PreferredLanguage ?? _serverSettings.DefaultLanguage ?? DefaultLanguage;

        return new CultureInfo(language);
    }

    public async Task<List<CultureInfo>> GetSupportedCultures()
    {
        var resourceFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Resources");

        return Directory.GetFiles(resourceFilePath, "SharedResource.*.resx")
            .Select(f => Path.GetFileNameWithoutExtension(f).Split('.')[1])
            .Append("en")
            .Select(CultureInfo.GetCultureInfo)
            .OrderBy(i => i.Name)
            .ToList();
    }
}
