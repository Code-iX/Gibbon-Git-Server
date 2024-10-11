using Gibbon.Git.Server.Data;

namespace Gibbon.Git.Server.Models;

public class TeamModel //: INameProperty
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public UserModel[] Members { get; set; }
    public string DisplayName => Name;
}
