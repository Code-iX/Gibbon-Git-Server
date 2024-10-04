using System.Threading.Tasks;

namespace Gibbon.Git.Server.Configuration;

public interface IServerSettingsService
{
    Task SaveSettings(ServerSettings settings);

    ServerSettings GetSettings();

    ServerSettings GetDefaultSettings();
    Task ResetSettings();
}

public interface IUserSettingsService
{
    Task SaveSettings(Guid userId, ServerSettings settings);

    ServerSettings GetSettings(Guid userId);

    ServerSettings GetDefaultSettings();
}

public class UserSettingsService : IUserSettingsService
{
    public Task SaveSettings(Guid userId, ServerSettings settings)
    {
        throw new NotImplementedException();
    }

    public ServerSettings GetSettings(Guid userId)
    {
        throw new NotImplementedException();
    }

    public ServerSettings GetDefaultSettings()
    {
        throw new NotImplementedException();
    }
}
