using System.Security.Claims;
using System.Security.Principal;

namespace Gibbon.Git.Server.Extensions;

public static class UserExtensions
{
    public static int Id(this IPrincipal user)
    {
        var id = user.GetClaimValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(id, out var result))
        {
            // It's a normal string Guid
            return result;
        }

        // TODO - This is the anonymous user, might be null
        return 0;
    }

    public static string Username(this IPrincipal user)
    {
        // We can tolerate the username being in either Upn or Name
        return user.GetClaimValue(ClaimTypes.Name) ?? user.GetClaimValue(ClaimTypes.Upn);
    }

    public static bool IsWindowsAuthenticated(this IPrincipal user)
    {
        var authenticationMethod = user.GetClaimValue(ClaimTypes.AuthenticationMethod);
        return !string.IsNullOrEmpty(authenticationMethod) && authenticationMethod.Equals("Windows", StringComparison.OrdinalIgnoreCase);
    }

    private static ClaimsIdentity GetClaimsIdentity(this IPrincipal user)
    {
        ClaimsIdentity result = null;

        if (user is ClaimsPrincipal claimsPrincipal)
        {
            result = claimsPrincipal.Identities.FirstOrDefault(x => x != null);
        }

        return result;
    }

    public static string StripDomain(this string username)
    {
        var delimiterIndex = username.IndexOf('@');
        if (delimiterIndex > 0)
        {
            username = username[..delimiterIndex];
        }
        delimiterIndex = username.IndexOf('\\');
        if (delimiterIndex > 0)
        {
            username = username[(delimiterIndex + 1)..];
        }

        return username;
    }

    public static string GetDomain(this string username)
    {
        var deliIndex = username.IndexOf('@');
        if (deliIndex > 0)
        {
            return username[(deliIndex + 1)..];
        }

        deliIndex = username.IndexOf('\\');
        if (deliIndex > 0)
        {
            return username[..deliIndex];
        }

        return string.Empty;
    }

    public static string StringlistToEscapedStringForEnvVar(this IEnumerable<string> items, string separator = ",")
    {
        var y = items.Select(x => x.Replace(@"\", @"\\").Replace(separator, @"\" + separator));
        return string.Join(separator, y);
    }

    private static string GetClaimValue(this IPrincipal user, string claimName)
    {
        try
        {
            return GetClaimsIdentity(user)?.FindFirst(claimName)?.Value;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
