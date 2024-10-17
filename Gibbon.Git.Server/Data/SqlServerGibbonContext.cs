using Gibbon.Git.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gibbon.Git.Server.Data;

public class SqlServerGibbonContext(DbContextOptions<SqlServerGibbonContext> options) : GibbonGitServerContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .Property(u => u.Username)
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Team>()
            .Property(t => t.Name)
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Role>()
            .Property(r => r.Name)
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Repository>()
            .Property(r => r.Name)
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");
    }
}