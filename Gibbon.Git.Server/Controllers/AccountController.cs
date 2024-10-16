using System.Threading.Tasks;

using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Extensions;
using Gibbon.Git.Server.Models;
using Gibbon.Git.Server.Provider;
using Gibbon.Git.Server.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Gibbon.Git.Server.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AccountController(IAuthenticationProvider authenticationProvider, IRoleProvider roleProvider, IUserService userService, IOptions<ApplicationSettings> options, ServerSettings serverSettings)
    : Controller
{
    private readonly ServerSettings _serverSettings = serverSettings;
    private readonly IUserService _userService = userService;
    private readonly IRoleProvider _roleProvider = roleProvider;
    private readonly IAuthenticationProvider _authenticationProvider = authenticationProvider;

    public IActionResult Index()
    {
        return View(GetDetailUsers());
    }

    private readonly ApplicationSettings _applicationSettings = options.Value;

    public IActionResult Detail(int id)
    {
        var user = _userService.GetUserModel(id);
        if (user != null)
        {
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
        return View();
    }

    public IActionResult Delete(int id)
    {
        var user = _userService.GetUserModel(id);
        if (user != null)
        {
            return View(user);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(UserDetailModel model)
    {
        if (model?.Id != null)
        {
            if (model.Id != User.Id())
            {
                var user = _userService.GetUserModel(model.Id);
                _userService.DeleteUser(user.Id);
                TempData["DeleteSuccess"] = true;
            }
            else
            {
                TempData["DeleteSuccess"] = false;
            }
        }
        return RedirectToAction("Index");
    }

    public IActionResult Edit(int id)
    {
        if (id != User.Id() && !User.IsInRole(Roles.Admin))
        {
            return Unauthorized();
        }

        var user = _userService.GetUserModel(id);
        if (user != null)
        {
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
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(UserEditModel model)
    {
        if (User.Id() != model.Id && !User.IsInRole(Roles.Admin))
        {
            return Unauthorized();
        }

        if (_applicationSettings.DemoModeActive && User.IsInRole(Roles.Admin) && User.Id() == model.Id)
        {
            // Don't allow the admin user to be changed in demo mode
            return Unauthorized();
        }

        if (ModelState.IsValid)
        {
            var valid = true;

            if (User.IsInRole(Roles.Admin) && model.Id == User.Id() && !(model.PostedSelectedRoles != null && model.PostedSelectedRoles.Contains(Roles.Admin)))
            {
                ModelState.AddModelError(nameof(model.Roles), Resources.Account_Edit_CannotRemoveYourselfFromAdminRole);
                valid = false;
            }

            if (valid)
            {
                _userService.UpdateUser(model.Id, model.Name, model.Surname, model.Email);
                // Only Administrators can make any changes to roles
                if (User.IsInRole(Roles.Admin))
                {
                    _roleProvider.RemoveRolesFromUser(model.Id, _roleProvider.GetAllRoles());
                    if (model.PostedSelectedRoles != null)
                    {
                        _roleProvider.AddRolesToUser(model.Id, model.PostedSelectedRoles);
                    }
                }
                ViewBag.UpdateSuccess = true;
            }
        }

        model.Roles = _roleProvider.GetAllRoles();
        model.SelectedRoles = model.PostedSelectedRoles;

        return View(model);
    }

    public IActionResult Create()
    {
        if (UserIsUnauthorized())
        {
            return Unauthorized();
        }

        return View();
    }

    private bool UserIsUnauthorized()
    {
        if (User.Identity.IsAuthenticated)
        {
            if (!User.IsInRole(Roles.Admin))
                return true;
        }
        else
        {
            if (!_serverSettings.AllowAnonymousRegistration)
                return true;
        }

        return false;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateModel model)
    {
        if (UserIsUnauthorized())
        {
            return Unauthorized();
        }

        model.Username = model.Username?.TrimEnd();

        if (!ModelState.IsValid)
            return View(model);

        if (!_userService.CreateUser(model.Username, model.Password, model.Name, model.Surname, model.Email))
        {
            ModelState.AddModelError(nameof(model.Username), Resources.Account_Create_AccountAlreadyExists);
            return View(model);
        }

        if (!User.IsInRole(Roles.Admin))
        {
            await _authenticationProvider.SignIn(model.Username, false);
            return RedirectToAction("Index", "Home");
        }

        TempData["CreateSuccess"] = true;
        TempData["NewUserId"] = _userService.GetUserModel(model.Username).Id;
        return RedirectToAction("Index");

    }

    private List<UserDetailModel> GetDetailUsers()
    {
        var users = _userService.GetAllUsers();
        var model = new List<UserDetailModel>
        {
        };
        foreach (var user in users)
        {
            model.Add(new UserDetailModel
            {
                Id = user.Id,
                Username = user.Username,
                Name = user.GivenName,
                Surname = user.Surname,
                Email = user.Email,
                Roles = _roleProvider.GetRolesForUser(user.Id)
            });
        }
        return model;
    }

}
