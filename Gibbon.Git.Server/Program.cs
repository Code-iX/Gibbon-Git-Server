using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Extensions;
using Gibbon.Git.Server.Git;
using Gibbon.Git.Server.Git.GitDownloadService;
using Gibbon.Git.Server.Git.GitService;
using Gibbon.Git.Server.Middleware;
using Gibbon.Git.Server.Middleware.Attributes;
using Gibbon.Git.Server.Middleware.Authorize;
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
services.AddScoped<IGitService, GitServiceExecutor>();
services.AddScoped<IGitDownloadService, GitDownloadService>();
services.AddScoped<GitControllerExceptionFilter>();

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
    options.Limits.MaxRequestBodySize = 102400 * 1024; // 100MB
    //options.ListenAnyIP(7274, listenOptions =>
    //{
    //    listenOptions.UseHttps(certificate);
    //});
});

services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 102400 * 1024; // 100MB
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

services.AddControllersWithViews(options =>
    {
        options.Filters.Add<AllViewsFilter>();
    })
    .AddCookieTempDataProvider();

var app = builder.Build();

//app.UseCertificateForKestrel();

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

//app.UseMiddleware<BasicAuthMiddleware>();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapRoutes();
app.UseMiddleware<CultureMiddleware>();

await app.RunAsync();

/*  AppSettings.IsPushAuditEnabled:
 *  
 *  var recoveryDirectory = ConfigurationManager.AppSettings["RecoveryDataPath"];
 *  var isReceivePackRecoveryProcessEnabled = !string.IsNullOrEmpty(recoveryDirectory);
 *  if (isReceivePackRecoveryProcessEnabled)
 *  {
 *      // git service execution durability registrations to enable receive-pack hook execution after failures
 *      MyDependencyResolver.RegisterType<IGitService, DurableGitServiceResult>();
 *      MyDependencyResolver.RegisterType<IHookReceivePack, ReceivePackRecovery>(); ### new NamedArguments.FailedPackWaitTimeBeforeExecution(TimeSpan.FromSeconds(5 * 60))
 *      MyDependencyResolver.RegisterType<IRecoveryFilePathBuilder, AutoCreateMissingRecoveryDirectories>();
 *      MyDependencyResolver.RegisterType<IRecoveryFilePathBuilder, OneFolderRecoveryFilePathBuilder>(); ### new NamedArguments.ReceivePackRecoveryDirectory(Path.IsPathRooted(recoveryDirectory) ? recoveryDirectory : HttpContext.Current.Server.MapPath(recoveryDirectory))
 *  }
 *
 *  // base git service executor
 *  MyDependencyResolver.RegisterType<IGitService, ReceivePackParser>();
 *  MyDependencyResolver.RegisterType<GitServiceResultParser>();
 *
 *  // receive pack hooks
 *  MyDependencyResolver.RegisterType<IHookReceivePack, AuditPusherToGitNotes>();
 *  MyDependencyResolver.RegisterType<IHookReceivePack, NullReceivePackHook>();
 *
 *  // run receive-pack recovery if possible
 *  if (isReceivePackRecoveryProcessEnabled)
 *  {
 *      var recoveryProcess = MyDependencyResolver.GetService<ReceivePackRecovery>(
 *              "failedPackWaitTimeBeforeExecution",
 *              new NamedArguments.FailedPackWaitTimeBeforeExecution(TimeSpan.FromSeconds(0))); // on start up set time to wait = 0 so that recovery for all waiting packs is attempted
 *
 *      try
 *      {
 *          recoveryProcess.RecoverAll();
 *      }
 *      catch
 *      {
 *          // don't let a failed recovery attempt stop start-up process
 *      }
 *  }
 */

// openssl req -x509 -newkey rsa:4096 -keyout mykey.key -out mycert.crt -days 365
