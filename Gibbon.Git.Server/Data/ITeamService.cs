using Gibbon.Git.Server.Models;

namespace Gibbon.Git.Server.Data;

public interface ITeamService
{
    List<TeamModel> GetAllTeams();
    TeamModel GetTeam(Guid id);
    List<TeamModel> GetTeamsForUser(Guid userId);
    bool IsTeamNameUnique(string name, Guid? existingTeamId = null);
    bool Create(TeamModel team);
    void Update(TeamModel team);
    void Delete(Guid id);
    void UpdateUserTeams(Guid userId, List<string> newTeams);
}
