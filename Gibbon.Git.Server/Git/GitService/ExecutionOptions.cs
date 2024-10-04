namespace Gibbon.Git.Server.Git.GitService;

public record ExecutionOptions(bool AdvertiseRefs, bool EndStreamWithClose = false)
{
    public string ToCommandLineArgs()
    {
        var args = "";
        if (AdvertiseRefs)
        {
            args += " --advertise-refs";
        }
        return args;
    }
}
