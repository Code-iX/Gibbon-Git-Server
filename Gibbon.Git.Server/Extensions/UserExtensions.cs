using System.Security.Claims;
using System.Security.Principal;

namespace Gibbon.Git.Server.Extensions;

public static class UserExtensions
{
    public static Guid Id(this IPrincipal user)
    {
        var id = user.GetClaimValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(id, out var result))
        {
            // It's a normal string Guid
            return result;
        }

        if (string.IsNullOrEmpty(id))
        {
            // This is the anonymous user
            return Guid.Empty;
        }
        try
        {
            // We might be a ADFS-style Guid is which a base64 string
            // If this fails, we'll get a FormatException thrown anyway
            return new Guid(Convert.FromBase64String(id));
        }
        catch (Exception)
        {
            return Guid.Empty;
        }
    }

    public static string Username(this IPrincipal user)
    {
        // We can tolerate the username being in either Upn or Name
        return user.GetClaimValue(ClaimTypes.Name) ?? user.GetClaimValue(ClaimTypes.Upn);
    }

    public static string DisplayName(this IPrincipal user)
    {
        return $"{user.GetClaimValue(ClaimTypes.GivenName)} {user.GetClaimValue(ClaimTypes.Surname)}";
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
