using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gibbon.Git.Server.Data;

/// <summary>
/// Provide reset services, to allow the database to be set to a known state
/// </summary>
public class DatabaseResetManager(ILogger<DatabaseResetManager> logger, IUserService users, ITeamService teamService, IRoleProvider roleProvider, IRepositoryService repository, IOptions<ApplicationSettings> options)
    
{
    private readonly ILogger<DatabaseResetManager> _logger = logger;
    private readonly IRepositoryService _repository = repository;
    private readonly ApplicationSettings _applicationSettings = options.Value;
    private readonly IRoleProvider _roleProvider = roleProvider;
    private readonly ITeamService _teamService = teamService;
    private readonly IUserService _users = users;

    public void DoReset(int mode)
    {
        _logger.LogInformation("Reset mode {mode}", mode);
        _logger.LogInformation("AppSettings {AllowDBReset}", _applicationSettings.AllowDbReset);
        switch (mode)
        {
            case 1:
                DoFullReset();
                break;

            default:
                throw new ArgumentException($"Requested invalid reset mode {mode}");
        }
    }

    /// <summary>
    /// Clear out everything except the admin user
    /// </summary>
    private void DoFullReset()
    {
        foreach (var repository in _repository.GetAllRepositories())
        {
            _repository.Delete(repository.Id);
        }
        foreach (var team in _teamService.GetAllTeams())
        {
            _teamService.Delete(team.Id);
        }
        foreach (var user in _users.GetAllUsers())
        {
            if (!user.Username.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                _users.DeleteUser(user.Id);
            }
        }
        foreach (var role in _roleProvider.GetAllRoles())
        {
            if (role != Definitions.Roles.Administrator)
            {
                _roleProvider.DeleteRole(role);
            }
        }
    }
}
