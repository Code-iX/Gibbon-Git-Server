using Gibbon.Git.Server.Data.Entities;
using Gibbon.Git.Server.Models;

using Microsoft.EntityFrameworkCore;

namespace Gibbon.Git.Server.Data;

public class TeamService(GibbonGitServerContext context) : ITeamService
{
    private readonly GibbonGitServerContext _context = context;

    public List<TeamModel> GetAllTeams()
    {
        var dbTeams = _context.Teams
            .AsSplitQuery()
            .Select(team => new
            {
                Id = team.Id,
                Name = team.Name,
                Description = team.Description,
                Members = team.Users,
                Repositories = team.Repositories.Select(m => m.Name),
            }).ToList();

        return dbTeams.Select(item => new TeamModel
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Members = item.Members.Select(user => user.ToModel()).ToArray(),
        }).ToList();
    }

    public List<TeamModel> GetTeamsForUser(Guid userId)
    {
        return GetAllTeams().Where(i => i.Members.Any(x => x.Id == userId)).ToList();
    }

    private static TeamModel GetTeamModel(Team team)
    {
        return team == null ? null : new TeamModel
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            Members = team.Users.Select(user => user.ToModel()).ToArray(),
        };
    }

    public TeamModel GetTeam(Guid id)
    {
        var team = _context.Teams
            .Include(x => x.Users)
            .FirstOrDefault(i => i.Id == id);

        return GetTeamModel(team);
    }

    public bool IsTeamNameUnique(string name, Guid? existingTeamId = null)
    {
        return !_context.Teams.Any(i => i.Name == name && (existingTeamId == null || i.Id != existingTeamId));
    }

    public void Delete(Guid teamId)
    {
        var team = _context.Teams.FirstOrDefault(i => i.Id == teamId);
        if (team != null)
        {
            team.Repositories.Clear();
            team.Users.Clear();
            _context.Teams.Remove(team);
            _context.SaveChanges();
        }
    }

    public bool Create(TeamModel model)
    {
        if (model == null) throw new ArgumentException("team");
        if (model.Name == null) throw new ArgumentException("name");

        // Write this into the model so that the caller knows the ID of the new itel
        model.Id = Guid.NewGuid();
        var team = new Team
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description
        };
        _context.Teams.Add(team);
        if (model.Members != null)
        {
            AddMembers(team, model.Members.Select(x => x.Id));
        }
        try
        {
            _context.SaveChanges();
            model.Id = team.Id;
        }
        catch (DbUpdateException)
        {
            return false;
        }
        catch (Exception)
        {
            // Not sure when this exception happens - DbUpdateException is what you get for adding a duplicate teamname
            return false;
        }

        return true;
    }

    public void Update(TeamModel model)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));
        ArgumentNullException.ThrowIfNull(model.Name, nameof(model.Name));

        var team = _context.Teams.Include(t => t.Users).FirstOrDefault(i => i.Id == model.Id);
        if (team == null)
        {
            return;
        }

        team.Name = model.Name;
        team.Description = model.Description;

        var existingUserIds = team.Users.Select(u => u.Id).ToList();
        var newUserIds = model.Members?.Select(m => m.Id).ToList() ?? [];

        var usersToRemove = team.Users.Where(u => !newUserIds.Contains(u.Id)).ToList();
        foreach (var user in usersToRemove)
        {
            team.Users.Remove(user);
        }

        AddMembers(team, newUserIds.Except(existingUserIds));

        _context.SaveChanges();
    }

    private void AddMembers(Team team, IEnumerable<Guid> members)
    {
        var users = _context.Users
            .Where(user => members.Contains(user.Id));

        foreach (var item in users)
        {
            team.Users.Add(item);
        }
    }

    public void UpdateUserTeams(Guid userId, List<string> newTeams)
    {
        ArgumentNullException.ThrowIfNull(newTeams, nameof(newTeams));

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.Teams.Clear();
            var teams = _context.Teams.Where(t => newTeams.Contains(t.Name));
            foreach (var team in teams)
            {
                user.Teams.Add(team);
            }
            _context.SaveChanges();
        }
    }
}
