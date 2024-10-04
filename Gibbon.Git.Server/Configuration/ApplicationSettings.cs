namespace Gibbon.Git.Server.Configuration;

public sealed record ApplicationSettings
{
    public bool IsPushAuditEnabled { get; set; }
    public bool AllowDbReset { get; set; }
    public bool DemoModeActive { get; set; }
    public string DataPath { get; set; }
    public string RepositoryPath { get; set; }

    public bool AllowDatabaseMigration { get; set; }
}
