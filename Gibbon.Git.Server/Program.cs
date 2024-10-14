using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Extensions;
using Gibbon.Git.Server.Git.GitDownloadService;
using Gibbon.Git.Server.Git.GitService;
using Gibbon.Git.Server.Git.GitVersionService;
using Gibbon.Git.Server.Middleware;
using Gibbon.Git.Server.Middleware.Attributes;
using Gibbon.Git.Server.Provider;
using Gibbon.Git.Server.Repositories;
using Gibbon.Git.Server.Security;
using Gibbon.Git.Server.Services;
using Gibbon.Git.Server.Services.Hosted;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var config = builder.Configuration;
var env = builder.Environment;

config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

services.Configure<ApplicationSettings>(config.GetSection("AppSettings"));
services.Configure<GitSettings>(config.GetSection("GitSettings"));
services.Configure<MailSettings>(config.GetSection("MailSettings"));

services.AddScoped(serviceProvider => serviceProvider.GetRequiredService<IServerSettingsService>().GetSettings());

services.AddDbContext<GibbonGitServerContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("GibbonGitServerContext"));
    options.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
});

services.AddLogging();
services.AddMemoryCache();

services.AddSingleton<IPathResolver, PathResolver>();
services.AddSingleton<IGitVersionService, GitVersionService>();
//services.AddSingleton<ICertificateService, CertificateService>();

services.AddScoped<IDatabaseHelperService, DatabaseHelperService>();
services.AddScoped<IPasswordService, PasswordService>();
services.AddScoped<ICultureService, CultureService>();
services.AddScoped<IUserService, UserService>();
services.AddScoped<IRoleProvider, RoleProvider>();
services.AddScoped<IAuthenticationProvider, CookieAuthenticationProvider>();
services.AddScoped<ITeamService, TeamService>();
services.AddScoped<IRepositoryService, RepositoryService>();
services.AddScoped<IRepositoryPermissionService, RepositoryPermissionService>();
services.AddScoped<IDiagnosticReporter, DiagnosticReporter>();
services.AddScoped<IRepositorySynchronizer, RepositorySynchronizer>();
services.AddScoped<IMailService, MailService>();
services.AddScoped<IServerSettingsService, ServerSettingsService>();
services.AddScoped<IUserSettingsService, UserSettingsService>();

services.AddScoped<IProcessService, ProcessService>();
services.AddScoped<IGitService, GitService>();
services.AddScoped<IGitDownloadService, GitDownloadService>();

services.AddScoped<IAvatarService, AvatarService>();
services.AddScoped<IRepositoryBrowserFactory, RepositoryBrowserFactory>();
services.AddTransient<IRepositoryBrowser, RepositoryBrowser>();

services.AddTransient<IStartupService, RepositoryStartupService>();
services.AddTransient<IStartupService, GitVersionStartupService>();

services.AddHostedService<DatabaseMigrationService>();
services.AddHostedService<StartupServiceRunner>();

services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();

services.AddHttpClient<IGitDownloadService, GitDownloadService>();

services.AddHttpContextAccessor();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
});

services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024;
});

services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "AntiForgery";
    options.FormFieldName = "_AntiForgery";
});

services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Home/Error";
        options.ExpireTimeSpan = TimeSpan.FromDays(3);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToAccessDenied = async context =>
        {
            context.Response.StatusCode = 403;
        };
        options.Events.OnRedirectToLogin = async context =>
        {
            if (!context.Request.Headers.ContainsKey("AuthNoRedirect"))
            {
                context.Response.Redirect(context.RedirectUri);
            }
        };
    });

services.AddAuthorization();

services.AddControllersWithViews()
    .AddCookieTempDataProvider();

var app = builder.Build();

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
app.UseExceptionHandler("/Home/Error");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = new FileExtensionContentTypeProvider
    {
        Mappings = { [".woff2"] = "font/woff2" }
    }
});
app.UseCookiePolicy();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapRoutes();
app.UseMiddleware<CultureMiddleware>();

await app.RunAsync();
