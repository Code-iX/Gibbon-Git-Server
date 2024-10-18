using Gibbon.Git.Server.Data;

using Microsoft.Extensions.DependencyInjection;

namespace Gibbon.Git.Server.Tests.TestHelper;

public abstract class DbTestBase<TDbConnectionFactory>
    : TestBase
    where TDbConnectionFactory : IDbConnectionFactory, new()
{
    private readonly TDbConnectionFactory _dbConnectionFactory = new();

    protected override void ConfigureServicesBase(ServiceCollection services)
    {
        _dbConnectionFactory.ConfigureService(services);
    }

    protected override void UseServicesBase(ServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<GibbonGitServerContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    [TestCleanup]
    public new void Cleanup()
    {
        base.Cleanup();
        _dbConnectionFactory.Cleanup();
    }

    protected GibbonGitServerContext DbContext => ServiceProvider.GetRequiredService<GibbonGitServerContext>();
}
