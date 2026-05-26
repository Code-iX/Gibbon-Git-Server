using Gibbon.Git.Server.Data.Entities;

namespace Gibbon.Git.Server.Configuration;

public sealed class UserSettings
{
    public string PreferredLanguage { get; set; }
    public NameFormat PreferredNameFormat { get; set; }
}
