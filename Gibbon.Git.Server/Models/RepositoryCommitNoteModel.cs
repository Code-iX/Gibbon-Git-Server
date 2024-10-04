namespace Gibbon.Git.Server.Models;

public class RepositoryCommitNoteModel
{
    public RepositoryCommitNoteModel(string message, string @namespace)
    {
        Message = message;
        Namespace = @namespace;
    }

    public string Message { get; set; }

    public string Namespace { get; set; }
}