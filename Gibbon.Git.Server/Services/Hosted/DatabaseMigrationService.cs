using System.Threading;
using System.Threading.Tasks;

using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gibbon.Git.Server.Services.Hosted;

public class DatabaseMigrationService(ILogger<DatabaseMigrationService> logger, IOptions<DatabaseSettings> options, IServiceProvider serviceProvider)
    : IHostedService
{
    private readonly ILogger<DatabaseMigrationService> _logger = logger;
    private readonly DatabaseSettings _options = options.Value;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<GibbonGitServerContext>();

        await context.Database.EnsureCreatedAsync(cancellationToken);

        if (!_options.AllowMigration)
        {
            _logger.LogInformation("Database migration is disabled.");
            return;
        }

        _logger.LogInformation("Migrating database...");
        var migrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
        var count = 0;
        foreach (var migration in migrations)
        {
            count++;
            _logger.LogInformation("Pending migration: {Migration}", migration);
        }

        if (count == 0)
        {
            _logger.LogInformation("No pending migrations.");
            return;
        }

        await context.Database.MigrateAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Nothing to do
    }
}
