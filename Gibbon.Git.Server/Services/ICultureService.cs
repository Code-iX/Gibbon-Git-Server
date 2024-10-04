using System.Globalization;
using System.Threading.Tasks;

namespace Gibbon.Git.Server.Services;

public interface ICultureService
{
    CultureInfo GetSelectedCultureInfo();

    Task<List<CultureInfo>> GetSupportedCultures();
    CultureInfo GetCultureForUser(Guid userId);
}
