using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Gibbon.Git.Server.Data.Factories;

public class SqlServerGibbonContextFactory : IDesignTimeDbContextFactory<SqlServerGibbonContext>
{
    public SqlServerGibbonContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SqlServerGibbonContext>();

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json")
            .Build();

        var connectionString = configuration.GetConnectionString("SqlServerGibbonContext");

        optionsBuilder.UseSqlServer(connectionString);

        return new SqlServerGibbonContext(optionsBuilder.Options);
    }
}
