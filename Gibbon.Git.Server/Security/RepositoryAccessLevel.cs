namespace Gibbon.Git.Server.Security;

public enum RepositoryAccessLevel
{
    /// <summary>
    /// User can read or clone a repository
    /// </summary>
    Pull,
    /// <summary>
    /// User can push to a repository
    /// </summary>
    Push,
    /// <summary>
    /// User can change repository settings
    /// </summary>
    Administer
}