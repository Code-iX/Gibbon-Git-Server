using Gibbon.Git.Server.Data.Entities;

namespace Gibbon.Git.Server.Models;

public class UserModel
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string GivenName { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }

    public string DisplayName => GetDisplayName(NameFormat.LastCommaFirst);

    /// <summary>
    /// This is the name we'd sort users by
    /// </summary>
    public string SortName => GetSortName(NameFormat.LastCommaFirst);

    public string GetDisplayName(NameFormat format)
    {
        if (string.IsNullOrWhiteSpace(GivenName) && string.IsNullOrWhiteSpace(Surname))
        {
            return Username;
        }

        return format switch
        {
            NameFormat.FirstLast => $"{GivenName} {Surname}".Trim(),
            NameFormat.LastCommaFirst => $"{Surname}, {GivenName}".Trim(' ', ','),
            NameFormat.LastFirst => $"{Surname} {GivenName}".Trim(),
            _ => $"{Surname}, {GivenName}".Trim(' ', ',')
        };
    }

    public string GetSortName(NameFormat format)
    {
        if (string.IsNullOrWhiteSpace(Surname) && string.IsNullOrWhiteSpace(GivenName))
        {
            return Username;
        }

        return format switch
        {
            NameFormat.FirstLast => $"{GivenName}{Surname}",
            NameFormat.LastCommaFirst => $"{Surname}{GivenName}",
            NameFormat.LastFirst => $"{Surname}{GivenName}",
            _ => $"{Surname}{GivenName}"
        };
    }
}

