using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Tests.TestHelper;

using Microsoft.EntityFrameworkCore;

namespace Gibbon.Git.Server.Tests.Data;

// TODO this is messed up
public abstract class GibbonDbContextTestsBase<TDbConnectionFactory>
    where TDbConnectionFactory : IDbConnectionFactory, new()
{
    private readonly TDbConnectionFactory _connectionFactory = new();

    protected GibbonGitServerContext Context { get; private set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        var optionsBuilder = new DbContextOptionsBuilder<SqliteGibbonContext>();
        _connectionFactory.ConfigureDbContext(optionsBuilder);
        Context = new SqliteGibbonContext(optionsBuilder.Options);
        Context.Database.EnsureCreated();
    }

    [TestCleanup]
    public void Cleanup()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
        _connectionFactory.Cleanup();
    }
}
