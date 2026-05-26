using System;
using System.Threading.Tasks;
using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Services;
using NSubstitute;

namespace Gibbon.Git.Server.Tests.Services;

[TestClass]
public class DateFormatServiceTests
{
    private IDateFormatService _dateFormatService = null!;
    private IUserSettingsService _userSettingsService = null!;
    private ServerSettings _serverSettings = null!;

    [TestInitialize]
    public void Init()
    {
        _userSettingsService = Substitute.For<IUserSettingsService>();
        _serverSettings = new ServerSettings
        {
            DefaultDateFormat = "yyyy-MM-dd",
            DefaultTimeFormat = "HH:mm:ss"
        };
        _dateFormatService = new DateFormatService(_userSettingsService, _serverSettings);
    }

    [TestMethod]
    public void FormatDate_WithNullDate_ReturnsEmptyString()
    {
        var result = _dateFormatService.FormatDate(null, "yyyy-MM-dd");
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void FormatDate_WithCustomFormat_ReturnsFormattedDate()
    {
        var date = new DateTime(2024, 1, 15);
        var result = _dateFormatService.FormatDate(date, "yyyy-MM-dd");
        Assert.AreEqual("2024-01-15", result);
    }

    [TestMethod]
    public void FormatDate_WithDifferentFormat_ReturnsFormattedDate()
    {
        var date = new DateTime(2024, 1, 15);
        var result = _dateFormatService.FormatDate(date, "dd.MM.yyyy");
        Assert.AreEqual("15.01.2024", result);
    }

    [TestMethod]
    public void FormatDate_WithEmptyFormat_UsesShortDateString()
    {
        var date = new DateTime(2024, 1, 15);
        var result = _dateFormatService.FormatDate(date, "");
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("1") && result.Contains("15") || result.Contains("2024"));
    }

    [TestMethod]
    public void FormatDateTime_WithNullDateTime_ReturnsEmptyString()
    {
        var result = _dateFormatService.FormatDateTime(null, "yyyy-MM-dd", "HH:mm:ss");
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void FormatDateTime_WithCustomFormats_ReturnsFormattedDateTime()
    {
        var dateTime = new DateTime(2024, 1, 15, 14, 30, 45);
        var result = _dateFormatService.FormatDateTime(dateTime, "yyyy-MM-dd", "HH:mm:ss");
        Assert.AreEqual("2024-01-15 14:30:45", result);
    }

    [TestMethod]
    public void FormatDateTime_With12HourFormat_ReturnsFormattedDateTime()
    {
        var dateTime = new DateTime(2024, 1, 15, 14, 30, 45);
        var result = _dateFormatService.FormatDateTime(dateTime, "MM/dd/yyyy", "hh:mm tt");
        Assert.AreEqual("01/15/2024 02:30 PM", result);
    }

    [TestMethod]
    public async Task FormatDate_UsesUserSettings()
    {
        var userSettings = new UserSettings { DateFormat = "dd.MM.yyyy" };
        _userSettingsService.GetSettings(1).Returns(Task.FromResult(userSettings));

        var date = new DateTime(2024, 1, 15);
        var result = await _dateFormatService.FormatDate(date, 1);
        Assert.AreEqual("15.01.2024", result);
    }

    [TestMethod]
    public async Task FormatDate_UsesServerDefaultWhenUserSettingIsNull()
    {
        var userSettings = new UserSettings { DateFormat = null };
        _userSettingsService.GetSettings(1).Returns(Task.FromResult(userSettings));

        var date = new DateTime(2024, 1, 15);
        var result = await _dateFormatService.FormatDate(date, 1);
        Assert.AreEqual("2024-01-15", result); // Server default is yyyy-MM-dd
    }

    [TestMethod]
    public async Task FormatDateTime_UsesUserSettings()
    {
        var userSettings = new UserSettings { DateFormat = "dd.MM.yyyy", TimeFormat = "HH:mm" };
        _userSettingsService.GetSettings(1).Returns(Task.FromResult(userSettings));

        var dateTime = new DateTime(2024, 1, 15, 14, 30, 45);
        var result = await _dateFormatService.FormatDateTime(dateTime, 1);
        Assert.AreEqual("15.01.2024 14:30", result);
    }

    [TestMethod]
    public async Task FormatDateTime_UsesServerDefaultsWhenUserSettingsAreNull()
    {
        var userSettings = new UserSettings { DateFormat = null, TimeFormat = null };
        _userSettingsService.GetSettings(1).Returns(Task.FromResult(userSettings));

        var dateTime = new DateTime(2024, 1, 15, 14, 30, 45);
        var result = await _dateFormatService.FormatDateTime(dateTime, 1);
        Assert.AreEqual("2024-01-15 14:30:45", result); // Server defaults are yyyy-MM-dd and HH:mm:ss
    }
}
