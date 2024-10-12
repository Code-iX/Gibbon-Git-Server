using Gibbon.Git.Server.Git.Models;

namespace Gibbon.Git.Server.Git;

public static class GitServiceResultParser
{
    public static GitExecutionResult ParseResult(Stream outputStream)
    {
        var hasError = true;
        if (outputStream.Length >= 10)
        {
            var buff5 = new byte[5];

            if (outputStream.Read(buff5, 0, buff5.Length) != buff5.Length)
            {
                throw new Exception("Unexpected number of bytes read");
            }
            if (outputStream.Read(buff5, 0, buff5.Length) != buff5.Length)
            {
                throw new Exception("Unexpected number of bytes read");
            }

            var firstChars = Encoding.ASCII.GetString(buff5);
            hasError = firstChars == "error";
        }

        return new GitExecutionResult(hasError);
    }
}
