using Gibbon.Git.Server.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace Gibbon.Git.Server.Data;

public class GibbonGitServerContext(DbContextOptions<GibbonGitServerContext> options) : DbContext(options)
{
    public DbSet<Repository> Repositories { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ServerSettingsEntity> ServerSettings { get; set; }
    public DbSet<UserSettingsEntity> UserSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GibbonGitServerContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<string>().UseCollation("NOCASE");
    }
}
