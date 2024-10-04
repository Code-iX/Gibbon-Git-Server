using System.Globalization;
using System.Threading.Tasks;
using Gibbon.Git.Server.Configuration;

using Microsoft.AspNetCore.Hosting;

namespace Gibbon.Git.Server.Services;

internal sealed class CultureService : ICultureService
{
    private readonly IServerSettingsService _serverSettingsService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CultureService(IServerSettingsService serverSettingsService, IWebHostEnvironment webHostEnvironment)
    {
        _serverSettingsService = serverSettingsService;
        _webHostEnvironment = webHostEnvironment;
    }

    public CultureInfo GetSelectedCultureInfo()
    {
        var settings = _serverSettingsService.GetSettings();
        return new CultureInfo(settings.DefaultLanguage);
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

    public CultureInfo GetCultureForUser(Guid userId)
    {   
        return GetSelectedCultureInfo();
    }
}
