using System;
using System.Security.Claims;
using Gibbon.Git.Server.Security;
using Gibbon.Git.Server.Tests.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Gibbon.Git.Server.Tests.ControllerTests;

public abstract class ControllerTestBase<TController> : TestBase where TController : Controller
{
    protected TController Controller { get; private set; } = null!;
    protected ClaimsPrincipal AdminUser { get; private set; } = null!;

    protected override void ConfigureServicesBase(ServiceCollection services)
    {
        services.AddSingleton(Substitute.For<ITempDataProvider>());

        AdminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Role, Definitions.Roles.Administrator)
        }, "TestAuthentication"));

        services.AddSingleton<TController>();
    }

    protected override void UseServices(IServiceProvider serviceProvider)
    {
        Controller = serviceProvider.GetRequiredService<TController>();
        Controller.TempData = new TempDataDictionary(new DefaultHttpContext(), serviceProvider.GetRequiredService<ITempDataProvider>());
        Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
            {
                User = AdminUser
            }
        };
    }
}
