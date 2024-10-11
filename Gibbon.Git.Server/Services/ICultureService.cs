using System.Globalization;
using System.Threading.Tasks;

namespace Gibbon.Git.Server.Services;

public interface ICultureService
{
    Task<CultureInfo> GetSelectedCultureInfo(Guid userId);

    Task<List<CultureInfo>> GetSupportedCultures();
}
