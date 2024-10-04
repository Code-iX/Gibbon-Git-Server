using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gibbon.Git.Server.Services.Hosted;

public class StartupServiceRunner(ILogger<StartupServiceRunner> logger, IServiceProvider serviceProvider)
    : IHostedService
{
    private readonly ILogger<StartupServiceRunner> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running start actions...");

        using var scope = _serviceProvider.CreateScope();
        var startActions = scope.ServiceProvider.GetServices<IStartupService>();
        foreach (var action in startActions)
        {
            _logger.LogInformation("Running: {0}", action.GetType().Name);
            await action.RunAsync();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Nothing to do
        // Later, we might add some Shutdown actions here
    }
}
