namespace Gibbon.Git.Server.Models;

public class UserModel
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string GivenName { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }

    public string DisplayName => !string.IsNullOrWhiteSpace(GivenName) || !string.IsNullOrWhiteSpace(Surname)
        ? $"{Surname}, {GivenName}".Trim()
        : Username;

    /// <summary>
    /// This is the name we'd sort users by
    /// </summary>
    public string SortName => !string.IsNullOrWhiteSpace(Surname) || !string.IsNullOrWhiteSpace(GivenName)
        ? $"{Surname}{GivenName}"
        : Username;
}
