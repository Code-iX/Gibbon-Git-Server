using Gibbon.Git.Server.Data;

namespace Gibbon.Git.Server.Configuration;

public sealed class UserSettings
{
    public string PreferredLanguage { get; set; }
    public RepositoryDefaultView? DefaultRepositoryView { get; set; }

}
