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

    // Abstrakte Methode, die von den abgeleiteten Klassen implementiert wird
    protected abstract void ConfigureServices(ServiceCollection services);

    // Abstrakte Methode, die von den abgeleiteten Klassen implementiert wird
    protected abstract void UseServices(IServiceProvider serviceProvider);
}
