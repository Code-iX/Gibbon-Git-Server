using Ude;

namespace Gibbon.Git.Server.Helpers;

public static class FileDisplayHandler
{
    public const string NoBrush = "nohighlight";

    private static readonly IReadOnlyDictionary<string, string> BrushMaps;

    static FileDisplayHandler()
    {
        var brushMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        Add(brushMap, "html", ".html", ".htm", ".xhtml", ".xslt", ".xml", ".asp", ".aspx", ".cshtml", ".xaml", ".csproj", ".config");
        Add(brushMap, "cpp", ".h", ".c", ".cpp");
        Add(brushMap, "diff", ".diff", ".patch");
        Add(brushMap, "erlang", ".erl", ".xlr", ".hlr");
        Add(brushMap, "js", ".js", ".jscript", ".javascript");
        Add(brushMap, "perl", ".pir", ".pm", ".pl");
        Add(brushMap, "ps", ".ps1", ".psm1");

        Add(brushMap, "vb", ".vb");
        Add(brushMap, "csharp", ".cs");
        Add(brushMap, "as3", ".as");
        Add(brushMap, "bash", ".sh");
        Add(brushMap, "cf", ".cf");
        Add(brushMap, "css", ".css");
        Add(brushMap, "delphi", ".pas");
        Add(brushMap, "groovy", ".groovy");
        Add(brushMap, "json", ".json");
        Add(brushMap, "java", ".java");
        Add(brushMap, "jfx", ".fx");
        Add(brushMap, "php", ".php");
        Add(brushMap, "python", ".py");
        Add(brushMap, "ruby", ".rb");
        Add(brushMap, "scala", ".scala");
        Add(brushMap, "sql", ".sql");
        Add(brushMap, "typescript", ".ts");

        BrushMaps = brushMap;
    }

    public static bool IsImage(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        try
        {
            return MimeTypes.GetMimeType(extension).Contains("image", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public static string GetBrush(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName, nameof(fileName));

        var extension = Path.GetExtension(fileName);

        return string.IsNullOrEmpty(extension) || !BrushMaps.TryGetValue(extension, out var brush)
            ? NoBrush
            : brush;
    }

    public static string GetText(byte[] data, Encoding encoding)
    {
        if (data == null || data.Length == 0)
        {
            return string.Empty;
        }

        using var memoryStream = new MemoryStream(data);
        using var reader = new StreamReader(memoryStream, encoding, detectEncodingFromByteOrderMarks: true);

        return reader.ReadToEnd();
    }

    public static bool TryGetEncoding(byte[] data, out Encoding encoding)
    {
        ICharsetDetector cdet = new CharsetDetector();
        cdet.Feed(data, 0, data.Length);
        cdet.DataEnd();
        if (cdet.Charset != null)
        {
            if (string.Equals(cdet.Charset, "big-5", StringComparison.OrdinalIgnoreCase))
            {
                encoding = Encoding.GetEncoding("big5");
                return true;
            }

            try
            {
                encoding = Encoding.GetEncoding(cdet.Charset);
                return true;
            }
            catch
            {
                encoding = Encoding.Default;
                return false;
            }
        }

        encoding = Encoding.Default;
        return false;
    }

    private static void Add(IDictionary<string, string> map, string brush, params string[] extensions)
    {
        foreach (var ext in extensions)
        {
            map[ext] = brush;
        }
    }
}
