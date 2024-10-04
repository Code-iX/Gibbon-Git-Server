namespace Gibbon.Git.Server.Configuration;

public sealed record GitSettings
{
    public string BinaryPath { get; set; }
    public string HomePath { get; set; }
    public string Version { get; set; }
    public string Architecture { get; set; }
}
