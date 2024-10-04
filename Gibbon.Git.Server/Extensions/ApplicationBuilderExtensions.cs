using Gibbon.Git.Server.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Gibbon.Git.Server.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void UseCertificateForKestrel(this WebApplication app)
    {
        var certificateService = app.Services.GetRequiredService<ICertificateService>();
        var certificate = certificateService.GetPublicCertificate();
        foreach (var url in app.Urls)
        {
            if (!url.StartsWith("https://"))
                continue;

            app.Use(async (context, next) =>
            {
                var kestrelOptions = app.Services.GetRequiredService<IWebHostBuilder>();
                kestrelOptions.UseKestrel(options =>
                {
                    options.ListenAnyIP(new Uri(url).Port, listenOptions =>
                    {
                        listenOptions.UseHttps(certificate);
                    });
                });
                await next();
            });
            break;
        }
    }
}
