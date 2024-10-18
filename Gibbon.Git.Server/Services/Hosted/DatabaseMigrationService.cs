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

public class DatabaseMigrationService(ILogger<DatabaseMigrationService> logger, IOptions<ApplicationSettings> options, IServiceProvider serviceProvider)
    : IHostedService
{
    private readonly ILogger<DatabaseMigrationService> _logger = logger;
    private readonly ApplicationSettings _options = options.Value;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.AllowDatabaseMigration)
        {
            _logger.LogInformation("Database migration is disabled.");
            return;
        }

        _logger.LogInformation("Migrating database...");         
        using IServiceScope scope = _serviceProvider.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<GibbonGitServerContext>();

        var migrations = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        foreach (var migration in migrations)
        {
            _logger.LogInformation("Pending migration: {Migration}", migration);
        }

        if (migrations.Count == 0)
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
