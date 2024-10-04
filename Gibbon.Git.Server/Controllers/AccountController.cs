using System.Threading.Tasks;

using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Extensions;
using Gibbon.Git.Server.Middleware.Authorize;
using Gibbon.Git.Server.Models;
using Gibbon.Git.Server.Provider;
using Gibbon.Git.Server.Security;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Gibbon.Git.Server.Controllers;

public class AccountController(IAuthenticationProvider authenticationProvider, IRoleProvider roleProvider, IMembershipService membershipService, IOptions<ApplicationSettings> options, ServerSettings serverSettings)
    : Controller
{
    private readonly ServerSettings _serverSettings = serverSettings;
    private readonly IMembershipService _membershipService = membershipService;
    private readonly IRoleProvider _roleProvider = roleProvider;
    private readonly IAuthenticationProvider _authenticationProvider = authenticationProvider;
    private readonly ApplicationSettings _applicationSettings = options.Value;

    [WebAuthorize]
    public IActionResult Detail(Guid id)
    {
        var user = _membershipService.GetUserModel(id);
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

    [WebAuthorize(Roles = Definitions.Roles.Administrator)]
    public IActionResult Delete(Guid id)
    {
        var user = _membershipService.GetUserModel(id);
        if (user != null)
        {
            return View(user);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [WebAuthorize(Roles = Definitions.Roles.Administrator)]
    public IActionResult Delete(UserDetailModel model)
    {
        if (model?.Id != null)
        {
            if (model.Id != User.Id())
            {
                var user = _membershipService.GetUserModel(model.Id);
                _membershipService.DeleteUser(user.Id);
                TempData["DeleteSuccess"] = true;
            }
            else
            {
                TempData["DeleteSuccess"] = false;
            }
        }
        return RedirectToAction("Index");
    }

    [WebAuthorize(Roles = Definitions.Roles.Administrator)]
    public IActionResult Index()
    {
        return View(GetDetailUsers());
    }

    [WebAuthorize]
    public IActionResult Edit(Guid id)
    {
        if (id != User.Id() && !User.IsInRole(Definitions.Roles.Administrator))
        {
            return Unauthorized();
        }

        var user = _membershipService.GetUserModel(id);
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
    [WebAuthorize]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(UserEditModel model)
    {
        if (User.Id() != model.Id && !User.IsInRole(Definitions.Roles.Administrator))
        {
            return Unauthorized();
        }

        if (_applicationSettings.DemoModeActive && User.IsInRole(Definitions.Roles.Administrator) && User.Id() == model.Id)
        {
            // Don't allow the admin user to be changed in demo mode
            return Unauthorized();
        }

        if (ModelState.IsValid)
        {
            var valid = true;

            if (User.IsInRole(Definitions.Roles.Administrator) && model.Id == User.Id() && !(model.PostedSelectedRoles != null && model.PostedSelectedRoles.Contains(Definitions.Roles.Administrator)))
            {
                ModelState.AddModelError(nameof(model.Roles), Resources.Account_Edit_CannotRemoveYourselfFromAdminRole);
                valid = false;
            }

            if (valid)
            {
                _membershipService.UpdateUser(model.Id, model.Username, model.Name, model.Surname, model.Email);
                // Only Administrators can make any changes to roles
                if (User.IsInRole(Definitions.Roles.Administrator))
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
        // Überprüfe, ob der Benutzer authentifiziert ist und nicht Administrator oder ob anonyme Registrierung deaktiviert ist
        if (UserIsUnauthorized())
        {
            return Unauthorized();
        }

        // Wenn der Benutzer berechtigt ist, zeige das View
        return View();
    }

    private bool UserIsUnauthorized()
    {
        if (User.Identity.IsAuthenticated)
        {
            if (!User.IsInRole(Definitions.Roles.Administrator))
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

        if (!_membershipService.CreateUser(model.Username, model.Password, model.Name, model.Surname, model.Email))
        {
            ModelState.AddModelError(nameof(model.Username), Resources.Account_Create_AccountAlreadyExists);
            return View(model);
        }

        if (!User.IsInRole(Definitions.Roles.Administrator))
        {
            await _authenticationProvider.SignIn(model.Username, false);
            return RedirectToAction("Index", "Home");
        }

        TempData["CreateSuccess"] = true;
        TempData["NewUserId"] = _membershipService.GetUserModel(model.Username).Id;
        return RedirectToAction("Index");

    }

    private List<UserDetailModel> GetDetailUsers()
    {
        var users = _membershipService.GetAllUsers();
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
