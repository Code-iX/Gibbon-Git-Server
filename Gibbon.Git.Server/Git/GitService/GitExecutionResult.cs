namespace Gibbon.Git.Server.Git.GitService;

public class GitExecutionResult
{
    public GitExecutionResult(bool hasError)
    {
        HasError = hasError;
    }

    public bool HasError { get; private set; }
}