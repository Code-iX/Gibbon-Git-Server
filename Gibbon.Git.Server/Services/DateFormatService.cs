using System;
using System.Globalization;
using System.Threading.Tasks;

using Gibbon.Git.Server.Configuration;

namespace Gibbon.Git.Server.Services;

internal sealed class DateFormatService(IUserSettingsService userSettingsService, ServerSettings serverSettings) : IDateFormatService
{
    private readonly IUserSettingsService _userSettingsService = userSettingsService;
    private readonly ServerSettings _serverSettings = serverSettings;

    public async Task<string> FormatDate(DateTime? date, int userId)
    {
        if (!date.HasValue)
        {
            return string.Empty;
        }

        var userSettings = userId != 0 
            ? await _userSettingsService.GetSettings(userId) 
            : _userSettingsService.GetDefaultSettings();

        var dateFormat = userSettings.DateFormat ?? _serverSettings.DefaultDateFormat;
        return FormatDate(date, dateFormat);
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
            return date.Value.ToShortDateString();
        }

        return date.Value.ToString(dateFormat);
    }

    public async Task<string> FormatDateTime(DateTime? dateTime, int userId)
    {
        if (!dateTime.HasValue)
        {
            return string.Empty;
        }

        var userSettings = userId != 0 
            ? await _userSettingsService.GetSettings(userId) 
            : _userSettingsService.GetDefaultSettings();

        var dateFormat = userSettings.DateFormat ?? _serverSettings.DefaultDateFormat;
        var timeFormat = userSettings.TimeFormat ?? _serverSettings.DefaultTimeFormat;
        
        return FormatDateTime(dateTime, dateFormat, timeFormat);
    }

    public string FormatDateTime(DateTime? dateTime, string dateFormat, string timeFormat)
    {
        if (!dateTime.HasValue)
        {
            return string.Empty;
        }

        var formattedDate = string.IsNullOrEmpty(dateFormat) 
            ? dateTime.Value.ToShortDateString() 
            : dateTime.Value.ToString(dateFormat);

        var formattedTime = string.IsNullOrEmpty(timeFormat)
            ? dateTime.Value.ToShortTimeString()
            : dateTime.Value.ToString(timeFormat);

        return $"{formattedDate} {formattedTime}";
    }
}
