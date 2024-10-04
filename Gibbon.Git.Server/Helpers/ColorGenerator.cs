using System.Security.Cryptography;

namespace Gibbon.Git.Server.Helpers;

public class ColorGenerator
{
    public static string GetColor<T>(T input)
    {
        byte[] hashBytes = ComputeSha256Hash(input.ToString());

        int r = (hashBytes[6] ^ hashBytes[9] + hashBytes[24]) % 256;
        int g = (hashBytes[3] ^ hashBytes[7] + hashBytes[13]) % 256;
        int b = (hashBytes[4] ^ hashBytes[17] + hashBytes[29]) % 256;

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    // SHA-256 Hash-Berechnung
    private static byte[] ComputeSha256Hash(string input)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
    }
}
