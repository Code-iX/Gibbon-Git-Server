namespace Gibbon.Git.Server.Git.GitService;

public record ExecutionOptions(bool AdvertiseRefs, bool EndStreamWithClose = false);
