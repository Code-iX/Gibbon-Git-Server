namespace Gibbon.Git.Server.Configuration;

public sealed record DatabaseSettings
{
    public bool AllowReset { get; set; }
    public bool AllowMigration { get; set; }
}
