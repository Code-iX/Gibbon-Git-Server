using System;
using Gibbon.Git.Server.Repositories;
using Gibbon.Git.Server.Tests.TestHelper;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Gibbon.Git.Server.Tests.Repositories;

[TestClass]
public class RepositoryBrowserFactoryTests : TestBase
{
    private IRepositoryBrowserFactory _factory = null!;
    private IRepositoryBrowser _repositoryBrowser = null!;

    protected override void ConfigureServices(ServiceCollection services)
    {
        services.AddSubstitute<IServiceProvider>();
        _repositoryBrowser = services.AddSubstitute<IRepositoryBrowser>();

        services.AddSingleton<RepositoryBrowserFactory>();
    }

    protected override void UseServices(IServiceProvider serviceProvider)
    {
        _factory = serviceProvider.GetRequiredService<RepositoryBrowserFactory>();
    }
    [TestMethod]

    public void Create_ShouldThrowArgumentException_WhenRepositoryPathIsNull()
    {
        // Act
        Assert.ThrowsException<ArgumentNullException>(() => _factory.Create(null));
    }

    [TestMethod]

    public void Create_ShouldThrowArgumentException_WhenRepositoryPathIsEmpty()
    {
        // Act
        Assert.ThrowsException<ArgumentException>(() => _factory.Create(string.Empty));
    }

    [TestMethod]
    public void Create_ShouldReturnRepositoryBrowser_WhenRepositoryPathIsValid()
    {
        // Arrange
        var validRepositoryPath = "valid/path/to/repository";

        // Act
        var result = _factory.Create(validRepositoryPath);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_repositoryBrowser, result);
        _repositoryBrowser.Received(1).SetRepository(validRepositoryPath);
    }
}
