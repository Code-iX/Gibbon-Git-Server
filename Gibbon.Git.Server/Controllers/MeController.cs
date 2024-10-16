using System.Threading.Tasks;

using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Extensions;
using Gibbon.Git.Server.Models;
using Gibbon.Git.Server.Security;
using Gibbon.Git.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Gibbon.Git.Server.Controllers;

[Authorize]
public class MeController(IUserService userService, IRoleProvider roleProvider, ICultureService cultureService, IUserSettingsService userSettingsService)
    : Controller
{
    private readonly IUserService _userService = userService;
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

        var model = new MeDetailModel
        {
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

        var user = _userService.GetUserModel(username);

        if (user == null)
        {
            return NotFound();
        }

        var model = new MeEditModel
        {
            Username = user.Username,
            Name = user.GivenName,
            Surname = user.Surname,
            Email = user.Email,
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(MeEditModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = GetCurrentUser();

        if (user == null)
        {
            return NotFound();
        }

        _userService.UpdateUser(user.Id, model.Name, model.Surname, model.Email);

        TempData["EditSuccess"] = true;
        return RedirectToAction("Edit");
    }

    [HttpGet]
    public IActionResult Password()
    {
        var username = User.Identity.Name;

        var user = _userService.GetUserModel(username);

        if (user == null)
        {
            return NotFound();
        }

        var model = new MePasswordModel();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Password(MePasswordModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (string.Equals(model.OldPassword, model.NewPassword, StringComparison.Ordinal))
        {
            ModelState.AddModelError(nameof(model.NewPassword), Resources.MeController_Password_MustBeDifferent);
            return View(model);
        }

        var username = User.Identity.Name;

        if (!_userService.IsPasswordValid(username, model.OldPassword))
        {
            ModelState.AddModelError(nameof(model.OldPassword), Resources.Account_Edit_OldPasswordIncorrect);
            return View(model);
        }

        var userId = User.Id();
        _userService.UpdatePassword(userId, model.NewPassword);
        
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

        cultureItems.Insert(0, new SelectListItem
        {
            Text = Resources.MeController_Settings_UseServerLanguage,
            Value = ""
        });

        var user = GetCurrentUser();

        var settings = await _userSettingsService.GetSettings(user.Id);

        return View(new MeSettingsModel
        {
            PreferredLanguage = settings.PreferredLanguage,
            AvailableLanguages = cultureItems
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(MeSettingsModel settings)
    {
        if (!ModelState.IsValid)
        {
            var cultures = await _cultureService.GetSupportedCultures();
            settings.AvailableLanguages = cultures
                .Select(cultureInfo => new SelectListItem
                {
                    Text = $"{cultureInfo.Name} - {cultureInfo.DisplayName}",
                    Value = cultureInfo.Name
                })
                .ToList();

            return View(settings);
        }

        var user = GetCurrentUser();

        await _userSettingsService.SaveSettings(user.Id, new UserSettings
        {
            PreferredLanguage = settings.PreferredLanguage
        });

        return RedirectToAction("Settings");
    }

    private UserModel GetCurrentUser()
    {
        var username = User.Identity.Name;
        return _userService.GetUserModel(username);
    }
}
