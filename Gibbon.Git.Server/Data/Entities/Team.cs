namespace Gibbon.Git.Server.Data.Entities;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public virtual ICollection<Repository> Repositories { get; set; } = new HashSet<Repository>();

    public virtual ICollection<User> Users { get; set; } = new HashSet<User>();
}
