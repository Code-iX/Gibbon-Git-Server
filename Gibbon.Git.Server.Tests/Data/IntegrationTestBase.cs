using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Tests.TestHelper;

using Microsoft.EntityFrameworkCore;

namespace Gibbon.Git.Server.Tests.Data;

public abstract class GibbonDbContextTestsBase<TConnectionFactory>
    where TConnectionFactory : IDbConnectionFactory, new()
{
    private IDbConnectionFactory _connectionFactory = null!;
    private GibbonGitServerContext _dbContext = null!;

    [TestInitialize]
    public void Initialize()
    {
        _connectionFactory = new TConnectionFactory();
        var optionsBuilder = new DbContextOptionsBuilder<SqliteGibbonContext>();
        _connectionFactory.ConfigureDbContext(optionsBuilder);
        _dbContext = new SqliteGibbonContext(optionsBuilder.Options);
        _dbContext.Database.EnsureCreated();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
        _connectionFactory.Cleanup();
    }

    protected GibbonGitServerContext Context => _dbContext;
}
