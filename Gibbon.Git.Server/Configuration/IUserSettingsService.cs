using System.Threading.Tasks;
using Gibbon.Git.Server.Migrations;

namespace Gibbon.Git.Server.Configuration;

public interface IUserSettingsService
{
    Task SaveSettings(Guid userId, UserSettings settings);

    Task<UserSettings> GetSettings(Guid userId);

    UserSettings GetDefaultSettings();
}
