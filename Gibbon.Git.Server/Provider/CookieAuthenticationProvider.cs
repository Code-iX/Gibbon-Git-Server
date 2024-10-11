using System.Security.Claims;
using System.Threading.Tasks;
using Gibbon.Git.Server.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Gibbon.Git.Server.Provider;

public class CookieAuthenticationProvider(IHttpContextAccessor httpContextAccessor, IUserService userService, IRoleProvider roleProvider)
    : IAuthenticationProvider
{
    private readonly IUserService _userService = userService;
    private readonly IRoleProvider _roleProvider = roleProvider;
    private readonly HttpContext _httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No HttpContext available.");

    public List<Claim> GetClaimsForUser(string username)
    {
        ArgumentNullException.ThrowIfNull(username, nameof(username));
        var user = _userService.GetUserModel(username);
        if (user == null)
        {
            return [];
        }

        List<Claim> result = [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.GivenName, user.GivenName),
            new(ClaimTypes.Surname, user.Surname),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, Definitions.Roles.Member)
        ];

        result.AddRange(_roleProvider.GetRolesForUser(user.Id).Select(x => new Claim(ClaimTypes.Role, x)));

        return result;
    }

    public async Task SignIn(string username, bool rememberMe)
    {
        var claims = GetClaimsForUser(username);
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe
        };

        await _httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProperties);
    }

    public async Task SignOut()
    {
        await _httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
