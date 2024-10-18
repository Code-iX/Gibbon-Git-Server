namespace Gibbon.Git.Server.Configuration;

public sealed record ApplicationSettings
{
    public bool DemoModeActive { get; set; }
    public string DataPath { get; set; }
    public string RepositoryPath { get; set; }
}
