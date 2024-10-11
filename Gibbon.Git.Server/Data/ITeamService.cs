using Gibbon.Git.Server.Models;

namespace Gibbon.Git.Server.Data;

public interface ITeamService
{
    List<TeamModel> GetAllTeams();
    TeamModel GetTeam(int id);
    List<TeamModel> GetTeamsForUser(int userId);
    bool IsTeamNameUnique(string name, int? existingTeamId = null);
    bool Create(TeamModel team);
    void Update(TeamModel team);
    void Delete(int id);
    void UpdateUserTeams(int userId, List<string> newTeams);
}
