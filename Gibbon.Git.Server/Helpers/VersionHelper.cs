namespace Gibbon.Git.Server.Helpers;

public static class VersionHelper
{
    public static string GetAssemblyVersion()
    {
        var version = typeof(Program).Assembly.GetName().Version;
        return version != null ? version.ToString() : "1.0.0.0";
    }
}