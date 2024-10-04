namespace Gibbon.Git.Server.Data.Entities;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public virtual ICollection<User> Users { get; set; } = new HashSet<User>();
}