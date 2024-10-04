using System.Security.Cryptography;

namespace Gibbon.Git.Server.Services;

public interface IAvatarService
{
    string GetAvatar(string email);
}

public class AvatarService : IAvatarService
{
    private const string GravatarBaseUrl = "https://www.gravatar.com/avatar/";
    private static readonly Dictionary<string, string> AvatarCache = new();

    private int _size = 65;

    public int Size
    {
        get => _size;
        set => _size = value;
    }

    public string GetAvatar(string email)
    {
        var lowerCaseEmail = email.ToLower().Trim();
        var key = $"{lowerCaseEmail}:{_size}";

        if (AvatarCache.TryGetValue(key, out var cachedAvatar))
        {
            return cachedAvatar;
        }

        var avatarUrl = GenerateGravatarUrl(lowerCaseEmail);
        AvatarCache[key] = avatarUrl;
        return avatarUrl;
    }

    private string GenerateGravatarUrl(string email)
    {
        using var sha256Hasher = SHA256.Create();
        var data = sha256Hasher.ComputeHash(Encoding.UTF8.GetBytes(email));
        var hashBuilder = new StringBuilder();

        foreach (var byteValue in data)
        {
            hashBuilder.Append(byteValue.ToString("x2"));
        }

        return $"{GravatarBaseUrl}{hashBuilder}?s={_size}";
    }
}
