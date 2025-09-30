using System;
using System.Globalization;
using System.Threading.Tasks;

using Gibbon.Git.Server.Configuration;

namespace Gibbon.Git.Server.Services;

internal sealed class DateFormatService(IUserSettingsService userSettingsService) : IDateFormatService
{
    private readonly IUserSettingsService _userSettingsService = userSettingsService;

    public async Task<string> FormatDate(DateTime? date, int userId)
    {
        if (!date.HasValue)
        {
            return string.Empty;
        }

        var userSettings = userId != 0 
            ? await _userSettingsService.GetSettings(userId) 
            : _userSettingsService.GetDefaultSettings();

        return FormatDate(date, userSettings.DateFormat);
    }

    public string FormatDate(DateTime? date, string dateFormat)
    {
        if (!date.HasValue)
        {
            return string.Empty;
        }

        if (string.IsNullOrEmpty(dateFormat))
        {
            // Use culture-specific default format
            return date.Value.ToString(CultureInfo.CurrentCulture);
        }

        return date.Value.ToString(dateFormat);
    }
}
