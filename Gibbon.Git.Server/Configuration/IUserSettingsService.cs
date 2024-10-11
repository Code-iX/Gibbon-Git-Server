using System.Threading.Tasks;

namespace Gibbon.Git.Server.Configuration;

public interface IUserSettingsService
{
    Task SaveSettings(int userId, UserSettings settings);

    Task<UserSettings> GetSettings(int userId);

    UserSettings GetDefaultSettings();
}
