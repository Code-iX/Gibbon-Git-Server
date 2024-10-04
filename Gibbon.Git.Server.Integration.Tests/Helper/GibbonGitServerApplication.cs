using System.Diagnostics;

using Gibbon.Git.Server.Git.GitDownloadService;
using Gibbon.Git.Server.Services;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gibbon.Git.Server.Integration.Tests.Helper;

internal class GibbonGitServerApplication : WebApplicationFactory<Program>
{
    private readonly string _fileName;
    public IMailService MailService { get; init; }
    public IGitDownloadService GitDownloadService { get; set; }

    public GibbonGitServerApplication(TestContext testContext)
    {
        Debug.Assert(testContext.TestRunDirectory != null, "TestContext.TestRunDirectory != null");
        _fileName = Path.Combine(testContext.TestRunDirectory, "Databases", testContext.TestName + ".db");
        Debug.WriteLine($"Database file: {_fileName}");

        var directory = Path.GetDirectoryName(_fileName);

        Directory.CreateDirectory(directory!);

        if (File.Exists(_fileName))
        {
            File.Delete(_fileName);
        }

        MailService = Substitute.For<IMailService>();
        GitDownloadService = Substitute.For<IGitDownloadService>();
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "ConnectionStrings:GibbonGitServerContext", $"Data Source={_fileName}" },
                { "AppSettings:AllowDatabaseMigration", "true" }
            };
            config.AddInMemoryCollection(inMemorySettings);
        });

        builder.ConfigureServices(services =>
        {
            services.AddScoped(_ => MailService);
            services.AddScoped(_ => GitDownloadService);
        });

        base.ConfigureWebHost(builder);
    }
}
