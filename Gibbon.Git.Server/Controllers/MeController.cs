using System.Threading.Tasks;

using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Extensions;
using Gibbon.Git.Server.Middleware.Authorize;
using Gibbon.Git.Server.Models;
using Gibbon.Git.Server.Security;
using Gibbon.Git.Server.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Gibbon.Git.Server.Controllers;

[WebAuthorize]
public class MeController(IMembershipService membershipService, IRoleProvider roleProvider, ICultureService cultureService, IUserSettingsService userSettingsService)
    : Controller
{
    private readonly IMembershipService _membershipService = membershipService;
    private readonly IRoleProvider _roleProvider = roleProvider;
    private readonly ICultureService _cultureService = cultureService;
    private readonly IUserSettingsService _userSettingsService = userSettingsService;

    [HttpGet]
    public IActionResult Index()
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return NotFound();
        }

        var model = new UserDetailModel
        {
            Id = user.Id,
            Username = user.Username,
            Name = user.GivenName,
            Surname = user.Surname,
            Email = user.Email,
            Roles = _roleProvider.GetRolesForUser(user.Id)
        };
        return View(model);
    }

    [HttpGet]
    public IActionResult Edit()
    {
        var username = User.Identity.Name;

        var user = _membershipService.GetUserModel(username);

        if (user == null)
        {
            return NotFound();
        }

        var model = new UserEditModel
        {
            Id = user.Id,
            Username = user.Username,
            Name = user.GivenName,
            Surname = user.Surname,
            Email = user.Email,
            Roles = _roleProvider.GetAllRoles(),
            SelectedRoles = _roleProvider.GetRolesForUser(user.Id)
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Password()
    {
        var username = User.Identity.Name;

        var user = _membershipService.GetUserModel(username);

        if (user == null)
        {
            return NotFound();
        }

        var model = new UserPasswordModel();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Password(UserPasswordModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (string.Equals(model.OldPassword, model.NewPassword, StringComparison.Ordinal))
        {
            ModelState.AddModelError(nameof(model.NewPassword), "Password must not be the same.");
            return View(model);
        }

        var username = User.Identity.Name;

        if (!_membershipService.IsPasswordValid(username, model.OldPassword))
        {
            ModelState.AddModelError(nameof(model.OldPassword), Resources.Account_Edit_OldPasswordIncorrect);
            return View(model);
        }

        var userId = User.Id();
        _membershipService.UpdatePassword(userId, model.NewPassword);
        
        TempData["PasswordChangeSuccess"] = true;
        return RedirectToAction("Password");
    }

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var cultures = await _cultureService.GetSupportedCultures();

        var cultureItems = cultures
            .Select(cultureInfo => new SelectListItem
            {
                Text = $"{cultureInfo.Name} - {cultureInfo.DisplayName}",
                Value = cultureInfo.Name
            })
            .ToList();

        var user = GetCurrentUser();

        var settings = "en";

        return View(new UserSettingsModel
        {
            DefaultLanguage = settings,
            AvailableLanguages = cultureItems
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(UserSettingsModel settings)
    {
        if (!ModelState.IsValid)
        {
            return View(settings);
        }

        var user = GetCurrentUser();
        return RedirectToAction("Settings");
    }

    private UserModel GetCurrentUser()
    {
        var username = User.Identity.Name;
        return _membershipService.GetUserModel(username);
    }
}
