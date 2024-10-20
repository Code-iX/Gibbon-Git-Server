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
    /// <para>The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"</para>
    /// </summary>
    public string GetFileSizeString(long size)
    {
        if (size == 0)
        {
            return "0 B";
        }

        string[] suffixes = ["B", "kB", "MB", "GB", "TB"];
        var readable = Math.Abs((double)size);
        var suffixIndex = 0;

        while (readable >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            readable /= 1024;
            suffixIndex++;
        }

        return readable switch
        {
            >= 100 => $"{readable:0} {suffixes[suffixIndex]}",
            > 10 => $"{readable:0.#} {suffixes[suffixIndex]}",
            _ => $"{readable:0.##} {suffixes[suffixIndex]}"
        };
    }
}
