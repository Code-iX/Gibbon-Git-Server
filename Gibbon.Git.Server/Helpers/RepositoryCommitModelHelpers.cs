using Gibbon.Git.Server.Models;

namespace Gibbon.Git.Server.Helpers;

public static class RepositoryCommitModelHelpers
{
    /// <summary>
    /// Create message
    /// </summary>
    /// <param name="message"></param>
    /// <param name="messageLengthLimit"></param>
    /// <returns></returns>
    public static RepositoryCommitTitleModel MakeCommitMessage(string message, int messageLengthLimit)
    {
        return BreakLine(message, messageLengthLimit);
    }

    /// <summary>
    /// Split a string to blocks by word
    /// </summary>
    /// <param name="title"></param>
    /// <param name="blockLength"></param>
    /// <returns></returns>
    private static RepositoryCommitTitleModel BreakLine(string title, int blockLength)
    {
        IEnumerable<string> words = title.Split(' ')
            .Select(el => el.Trim())
            .Where(el => !string.IsNullOrEmpty(el)).ToArray();

        var message = new List<string>();
        var preBlock = new List<string>();
        var addToPreMessage = false;

        var currentLen = 0;

        foreach (var word in words)
        {
            if (addToPreMessage)
            {
                preBlock.Add(word);
            }
            else
            {
                message.Add(word);
            }
            currentLen += word.Length;

            if (currentLen >= blockLength)
            {
                currentLen = 0;
                addToPreMessage = true;
            }
        }

        var messageString = string.Join(" ", message.ToArray()).Trim();
        var preBlockString = string.Join(" ", preBlock.ToArray()).Trim();

        return new RepositoryCommitTitleModel
        {
            ShortTitle = messageString,
            ExtraTitle = preBlockString
        };
    }
}