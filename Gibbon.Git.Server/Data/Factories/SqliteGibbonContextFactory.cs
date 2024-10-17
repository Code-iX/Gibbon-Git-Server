using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Gibbon.Git.Server.Data.Factories;

public class SqliteGibbonContextFactory : IDesignTimeDbContextFactory<SqliteGibbonContext>
{
    public SqliteGibbonContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SqliteGibbonContext>();

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json")
            .Build();

        var connectionString = configuration.GetConnectionString("SqliteGibbonContext");

        optionsBuilder.UseSqlite(connectionString);

        return new SqliteGibbonContext(optionsBuilder.Options);
    }
}