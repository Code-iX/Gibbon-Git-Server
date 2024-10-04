using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Models;

public class RepositoryTreeDetailModel
{
    [Display(ResourceType = typeof(Resources), Name = "Repository_Tree_Name")]
    public string Name { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Repository_Tree_CommitMessage")]
    public string CommitMessage { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Repository_Tree_CommitDate")]
    public DateTime? CommitDate { get; set; }
    public string CommitDateString { get { return CommitDate.HasValue ? CommitDate.Value.ToString() : CommitDate.ToString(); } }

    [Display(ResourceType = typeof(Resources), Name = "Repository_Tree_Author")]
    public string Author { get; set; }
    public bool IsTree { get; set; }
    public bool IsLink { get; set; }
    public string TreeName { get; set; }
    public bool IsImage { get; set; }
    public bool IsText { get; set; }
    public bool IsMarkdown { get; set; }
    public string Path { get; set; }
    public byte[] Data { get; set; }
    public string Text { get; set; }
    public string TextBrush { get; set; }
    public Encoding Encoding { get; set; }
    public RepositoryLogoDetailModel Logo { get; set; }
}