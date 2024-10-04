using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Helpers;
using Gibbon.Git.Server.Middleware.Authorize;
using Gibbon.Git.Server.Models;
using Gibbon.Git.Server.Provider;
using Gibbon.Git.Server.Security;
using Gibbon.Git.Server.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Gibbon.Git.Server.Controllers;

public class HomeController(ILogger<HomeController> logger, IMembershipService membershipService, IAuthenticationProvider authenticationProvider, IMemoryCache memoryCache, GibbonGitServerContext dbContext, IDiagnosticReporter diagnosticReporter, IMailService mailService)
    : Controller
{
    private readonly ILogger<HomeController> _logger = logger;
    private readonly IMembershipService _membershipService = membershipService;
    private readonly IAuthenticationProvider _authenticationProvider = authenticationProvider;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly GibbonGitServerContext _dbContext = dbContext;
    private readonly IDiagnosticReporter _diagnosticReporter = diagnosticReporter;
    private readonly IMailService _mailService = mailService;

    [WebAuthorize]
    public IActionResult Index() => RedirectToAction("Index", "Repository");

    public IActionResult Error() => View();

    public IActionResult Login(string returnUrl)
    {
        return View(new LoginModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginModel model)
    {        
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        bool validationResult;
        try
        {
            validationResult = _membershipService.IsPasswordValid(model.Username, model.Password);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user validation.");
            ModelState.AddModelError("", Resources.Home_Login_FailureDuringValidation);
            return View(model);
        }

        if (!validationResult)
        {
            ModelState.AddModelError("", Resources.Home_Login_UsernamePasswordIncorrect);
            return View(model);
        }

        await _authenticationProvider.SignIn(model.Username, model.RememberMe);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [WebAuthorize]
    public async Task<IActionResult> Logout()
    {
        await _authenticationProvider.SignOut();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult ResetPassword(string digest)
    {
        if (string.IsNullOrEmpty(digest))
        {
            return BadRequest();
        }

        var username = CheckForPasswordResetUsername(digest);
        if (username == null)
        {
            return Forbid();
        }

        if (!_dbContext.Users.Any(x => x.Username == username))
        {
            return Forbid();
        }

        return View(new ResetPasswordModel
        {
            Username = username,
            Digest = digest
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ResetPassword(ResetPasswordModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var username = CheckForPasswordResetUsername(model.Digest);
        if (username == null || username != model.Username)
        {
            throw new UnauthorizedAccessException("Invalid password reset form");
        }

        var user = _dbContext.Users.FirstOrDefault(x => x.Username == model.Username);
        if (user == null)
        {
            TempData["ResetSuccess"] = false;
            return View(model);
        }

        _membershipService.UpdatePassword(user.Id, model.Password);
        TempData["ResetSuccess"] = true;

        _memoryCache.Remove(model.Digest);

        return RedirectToAction("Login");
    }

    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(ForgotPasswordModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = _membershipService.GetUserModel(model.Username);
        if (user == null)
        {
            ModelState.AddModelError("", Resources.Home_ForgotPassword_UserNameFailure);
            return View(model);
        }

        var token = _membershipService.GenerateResetToken(user.Username);

        _memoryCache.Set(token, model.Username, TimeSpan.FromHours(1));
        var resetUrl = Url.Action("ResetPassword", "Home", new { digest = token }, Request.Scheme);

        TempData["SendSuccess"] = _mailService.SendForgotPasswordEmail(user, resetUrl);
        return RedirectToAction("Login");
    }

    public IActionResult Diagnostics()
    {
        if (!HttpContext.IsLocalRequest())
        {
            return Content("You can only run the diagnostics locally to the server");
        }

        return Content(_diagnosticReporter.GetVerificationReport(), "text/plain", Encoding.UTF8);
    }

    private string CheckForPasswordResetUsername(string decodedDigest)
    {
        if (!_memoryCache.TryGetValue(decodedDigest, out var cacheObj))
        {
            return null;
        }

        return cacheObj?.ToString();
    }

}
