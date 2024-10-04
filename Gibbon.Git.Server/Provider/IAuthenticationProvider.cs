using System.Security.Claims;
using System.Threading.Tasks;

namespace Gibbon.Git.Server.Provider;

public interface IAuthenticationProvider
{
    Task SignIn(string username, bool rememberMe);

    Task SignOut();

    List<Claim> GetClaimsForUser(string username);
}
