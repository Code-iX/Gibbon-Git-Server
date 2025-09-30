using System.Threading.Tasks;

namespace Gibbon.Git.Server.Services;

public interface IDateFormatService
{
    Task<string> FormatDate(DateTime? date, int userId);
    string FormatDate(DateTime? date, string dateFormat);
}
