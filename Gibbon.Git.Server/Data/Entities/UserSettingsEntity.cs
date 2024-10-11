﻿namespace Gibbon.Git.Server.Data.Entities;

public class UserSettingsEntity
{
    public Guid UserId { get; set; }
    public string PreferredLanguage { get; set; }
    public ThemeMode PreferredThemeMode { get; set; }
    public bool ReceiveEmailNotifications { get; set; }
    public string TimeZone { get; set; }
    public string DateFormat { get; set; }
    public string DefaultHomePage { get; set; }
}
