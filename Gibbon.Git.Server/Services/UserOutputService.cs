using System.Globalization;

namespace Gibbon.Git.Server.Services;

public interface IUserOutputService
{
    /// <summary>
    /// <para>Returns the human-readable file size for an arbitrary, 64-bit file size</para>
    /// <para>The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"</para>
    /// </summary>
    string GetFileSizeString(long size);
}

public class UserOutputService : IUserOutputService
{
    /// <summary>
    /// <para>Returns the human-readable file size for an arbitrary, 64-bit file size</para>
    /// </summary>
    public string GetFileSizeString(long size)
    {
        // TB is more than enough for a 64-bit file size
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        var readable = Math.Abs((double)size);
        var suffixIndex = 0;

        while (readable >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            // even we divide by 1024, we still use 1000 as the threshold
            readable /= 1024;
            suffixIndex++;
        }

        if (suffixIndex == 0)
        {
            return $"{readable.ToString("F0", CultureInfo.InvariantCulture)} {suffixes[suffixIndex]}";
        }

        return readable switch
        {
            >= 100 => $"{readable.ToString("F0", CultureInfo.InvariantCulture)} {suffixes[suffixIndex]}",
            >= 10 => $"{readable.ToString("F1", CultureInfo.InvariantCulture)} {suffixes[suffixIndex]}",
            _ => $"{readable.ToString("F2", CultureInfo.InvariantCulture)} {suffixes[suffixIndex]}"
        };
    }
}
