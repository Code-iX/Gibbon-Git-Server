using System;
using Microsoft.Extensions.DependencyInjection;

namespace Gibbon.Git.Server.Tests.TestHelper;

public abstract class TestBase
{
    protected ServiceProvider ServiceProvider = null!;

    [TestInitialize]
    public void Initialize()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        ConfigureServicesBase(services);
        ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();
        UseServicesBase(ServiceProvider);
        UseServices(ServiceProvider);
    }

    protected virtual void UseServicesBase(ServiceProvider serviceProvider) { }

    protected virtual void ConfigureServicesBase(ServiceCollection services) { }

    [TestCleanup]
    public void Cleanup()
    {
        ServiceProvider.Dispose();
    }

    protected abstract void ConfigureServices(ServiceCollection services);

    protected abstract void UseServices(IServiceProvider serviceProvider);
}
