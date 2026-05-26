namespace Gibbon.Git.Server.Helpers;

public static class IdeHelper
{
    public static string GetIdeDisplayName(Configuration.IdeType ideType)
    {
        return ideType switch
        {
            Configuration.IdeType.None => "None",
            Configuration.IdeType.VisualStudio => "Visual Studio",
            Configuration.IdeType.VisualStudioCode => "Visual Studio Code",
            Configuration.IdeType.AndroidStudio => "Android Studio",
            Configuration.IdeType.CLion => "CLion",
            Configuration.IdeType.DataGrip => "DataGrip",
            Configuration.IdeType.Eclipse => "Eclipse",
            Configuration.IdeType.IntelliJIdea => "IntelliJ IDEA",
            Configuration.IdeType.Rider => "Rider",
            Configuration.IdeType.PhpStorm => "PhpStorm",
            Configuration.IdeType.PyCharm => "PyCharm",
            Configuration.IdeType.RubyMine => "RubyMine",
            Configuration.IdeType.Tower => "Tower",
            Configuration.IdeType.WebStorm => "WebStorm",
            Configuration.IdeType.GoLand => "GoLand",
            Configuration.IdeType.AppCode => "AppCode",
            Configuration.IdeType.Fleet => "Fleet",
            Configuration.IdeType.RustRover => "RustRover",
            Configuration.IdeType.Aqua => "Aqua",
            _ => "Unknown"
        };
    }

    public static string GetIdeProtocol(Configuration.IdeType ideType, string gitUrl)
    {
        // URL encode the git URL for use in IDE protocols
        var encodedUrl = Uri.EscapeDataString(gitUrl);
        
        return ideType switch
        {
            Configuration.IdeType.VisualStudio => $"git-client://clone?repo={encodedUrl}",
            Configuration.IdeType.VisualStudioCode => $"vscode://vscode.git/clone?url={encodedUrl}",
            Configuration.IdeType.AndroidStudio => $"androidstudio://git-clone?url={encodedUrl}",
            Configuration.IdeType.CLion => $"jetbrains://clion/git-clone?url={encodedUrl}",
            Configuration.IdeType.DataGrip => $"jetbrains://datagrip/git-clone?url={encodedUrl}",
            Configuration.IdeType.IntelliJIdea => $"jetbrains://idea/git-clone?url={encodedUrl}",
            Configuration.IdeType.Rider => $"jetbrains://rider/git-clone?url={encodedUrl}",
            Configuration.IdeType.PhpStorm => $"jetbrains://phpstorm/git-clone?url={encodedUrl}",
            Configuration.IdeType.PyCharm => $"jetbrains://pycharm/git-clone?url={encodedUrl}",
            Configuration.IdeType.RubyMine => $"jetbrains://rubymine/git-clone?url={encodedUrl}",
            Configuration.IdeType.WebStorm => $"jetbrains://webstorm/git-clone?url={encodedUrl}",
            Configuration.IdeType.GoLand => $"jetbrains://goland/git-clone?url={encodedUrl}",
            Configuration.IdeType.AppCode => $"jetbrains://appcode/git-clone?url={encodedUrl}",
            Configuration.IdeType.Fleet => $"jetbrains://fleet/git-clone?url={encodedUrl}",
            Configuration.IdeType.RustRover => $"jetbrains://rustrover/git-clone?url={encodedUrl}",
            Configuration.IdeType.Aqua => $"jetbrains://aqua/git-clone?url={encodedUrl}",
            Configuration.IdeType.Tower => $"gittower://openRepo/{encodedUrl}",
            Configuration.IdeType.Eclipse => gitUrl, // Eclipse doesn't have a custom protocol
            _ => gitUrl
        };
    }
}
