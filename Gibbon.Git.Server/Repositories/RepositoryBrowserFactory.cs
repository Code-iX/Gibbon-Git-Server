using Microsoft.Extensions.DependencyInjection;

namespace Gibbon.Git.Server.Repositories;

public class RepositoryBrowserFactory(IServiceProvider serviceProvider) : IRepositoryBrowserFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public IRepositoryBrowser Create(string repositoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath, nameof(repositoryPath));
        var browser = _serviceProvider.GetRequiredService<IRepositoryBrowser>();
        browser.SetRepository(repositoryPath);
        return browser;
    }
}