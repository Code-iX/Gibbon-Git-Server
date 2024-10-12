namespace Gibbon.Git.Server.Git.Models;

public record ExecutionOptions(bool AdvertiseRefs, bool EndStreamWithClose = false);
