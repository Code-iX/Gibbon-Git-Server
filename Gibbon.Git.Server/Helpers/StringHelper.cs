using System.Text.RegularExpressions;

namespace Gibbon.Git.Server.Helpers;

public static partial class StringHelper
{
    public static string RemoveWhiteSpace(string input)
    {
        return WhiteSpaceRemoveRegex().Replace(input, string.Empty);
    }

    public static string RemoveIllegalChars(string input)
    {
        return IllegalChars().Replace(input, "");
    }

    public static bool NameIsValid(string value)
    {
        return NameIsValidRegex().IsMatch(value) && value.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
    }

    [GeneratedRegex(@"^([\w\.-])*([\w])$")]
    private static partial Regex NameIsValidRegex();

    [GeneratedRegex(@"\s")]
    private static partial Regex WhiteSpaceRemoveRegex();

    [GeneratedRegex("([/\\:*?\"<>|])")]
    private static partial Regex IllegalChars();
}
