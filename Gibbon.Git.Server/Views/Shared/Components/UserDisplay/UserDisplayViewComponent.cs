using System.Threading.Tasks;

using Gibbon.Git.Server.Extensions;
using Gibbon.Git.Server.Security;

using Microsoft.AspNetCore.Mvc;

namespace Gibbon.Git.Server.Views.Shared.Components.UserDisplay;

public class UserDisplayViewComponent(IUserService userService) : ViewComponent
{
    private readonly IUserService _userService = userService;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userModel = _userService.GetUserModel(User.Id());

        return View(userModel);
    }
}
