using Gibbon.Git.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gibbon.Git.Server.Data;

public class SqliteGibbonContext(DbContextOptions<SqliteGibbonContext> options) : GibbonGitServerContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .Property(u => u.Username)
            .UseCollation("NOCASE");

        modelBuilder.Entity<Team>()
            .Property(t => t.Name)
            .UseCollation("NOCASE");

        modelBuilder.Entity<Role>()
            .Property(r => r.Name)
            .UseCollation("NOCASE");

        modelBuilder.Entity<Repository>()
            .Property(r => r.Name)
            .UseCollation("NOCASE");
    }
}