using System.Security.Cryptography;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Gibbon.Git.Server.Security;

public class PasswordService : IPasswordService
{
    private const int SaltSize = 128 / 8;
    private const int TokenSize = 256 / 8;
    private const int HashSize = 512 / 8;
    private const int IterationCount = 10000;

    public string GenerateSalt()
    {
        var salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return Convert.ToBase64String(salt);
    }

    public string GenerateToken(string value)
    {
        byte[] salt;
        byte[] subkey;

        using (var deriveBytes = new Rfc2898DeriveBytes(value, SaltSize, IterationCount, HashAlgorithmName.SHA256))
        {
            salt = deriveBytes.Salt;
            subkey = deriveBytes.GetBytes(TokenSize);
        }

        var outputBytes = new byte[1 + SaltSize + TokenSize];
        Buffer.BlockCopy(salt, 0, outputBytes, 1, SaltSize);
        Buffer.BlockCopy(subkey, 0, outputBytes, 1 + SaltSize, TokenSize);
        return Convert.ToBase64String(outputBytes);
    }

    public string GenerateHash(string salt, string value)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hashBytes = KeyDerivation.Pbkdf2(value, saltBytes, KeyDerivationPrf.HMACSHA512, IterationCount, HashSize);
        return Convert.ToBase64String(hashBytes);
    }
    public bool CompareHash(string salt, string password, string knownHash)
    {
        var computedHash = GenerateHash(salt, password);
        return string.Compare(knownHash, computedHash, StringComparison.Ordinal) == 0;
    }
}
