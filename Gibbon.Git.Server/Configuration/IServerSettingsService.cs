using System.Threading.Tasks;

namespace Gibbon.Git.Server.Configuration;

public interface IServerSettingsService
{
    Task SaveSettings(ServerSettings settings);

    ServerSettings GetSettings();

    ServerSettings GetDefaultSettings();
    Task ResetSettings();
}
