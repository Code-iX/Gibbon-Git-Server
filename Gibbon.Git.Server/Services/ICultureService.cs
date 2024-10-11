using System.Globalization;
using System.Threading.Tasks;

namespace Gibbon.Git.Server.Services;

public interface ICultureService
{
    Task<CultureInfo> GetSelectedCultureInfo(int userId);

    Task<List<CultureInfo>> GetSupportedCultures();
}
