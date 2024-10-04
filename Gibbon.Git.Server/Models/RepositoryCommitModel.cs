using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Models;

public class RepositoryCommitModel
{
    public string Name { get; set; }
    public RepositoryLogoDetailModel Logo { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_ID")]
    public string ID { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_TreeID")]
    public string TreeID { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_Parents")]
    public string[] Parents { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_Author")]
    public string Author { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_AuthorEmail")]
    public string AuthorEmail { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_AuthorAvatar")]
    public string AuthorAvatar { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_Date")]
    public DateTime Date { get; set; }

    private string _message;
    [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_Message")]

    public string Message
    {
        get => string.IsNullOrEmpty(_message) ? Resources.Repository_Commit_NoMessageDeclared : _message;
        set => _message = value;
    }

    public string MessageShort { get; set; }

    public IEnumerable<string> Tags { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Repository_Commit_Changes")]
    public IEnumerable<RepositoryCommitChangeModel> Changes { get; set; }

    public IEnumerable<RepositoryCommitNoteModel> Notes { get; set; }

    public IEnumerable<string> Links { get; set; } = new List<string>();
}