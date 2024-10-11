using Gibbon.Git.Server.Data.Entities;
using Gibbon.Git.Server.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gibbon.Git.Server.Data;

public class RepositoryService(ILogger<RepositoryService> logger, GibbonGitServerContext context)
    : IRepositoryService
{
    private readonly GibbonGitServerContext _context = context;
    private readonly ILogger<RepositoryService> _logger = logger;

    public List<RepositoryModel> GetAllRepositories()
    {
        var dbrepos = _context.Repositories
            .AsNoTracking()
            .Include(repo => repo.Administrators)
            .Include(repo => repo.Teams)
            .Include(repo => repo.Users)
            .AsSplitQuery()
            .Select(repo => new
            {
                Id = repo.Id,
                Name = repo.Name,
                Group = repo.Group,
                Description = repo.Description,
                AnonymousAccess = repo.Anonymous,
                Users = repo.Users,
                Teams = repo.Teams,
                Administrators = repo.Administrators,
                AuditPushUser = repo.AuditPushUser,
                AllowAnonPush = repo.AllowAnonymousPush,
                Logo = repo.Logo
            }).ToList();

        return dbrepos.Select(repo => new RepositoryModel
        {
            Id = repo.Id,
            Name = repo.Name,
            Group = repo.Group,
            Description = repo.Description,
            AnonymousAccess = repo.AnonymousAccess,
            Users = repo.Users.Select(user => user.ToModel()).ToArray(),
            Teams = repo.Teams.Select(TeamToTeamModel).ToArray(),
            Administrators = repo.Administrators.Select(user => user.ToModel()).ToArray(),
            AuditPushUser = repo.AuditPushUser,
            AllowAnonymousPush = repo.AllowAnonPush,
            Logo = repo.Logo
        }).ToList();
    }

    public RepositoryModel GetRepository(string name)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));

        /* The straight-forward solution of using FindFirstOrDefault with
         * string.Equal does not work. Even name.Equals with OrdinalIgnoreCase does not
         * as it seems to get translated into some specific SQL syntax and EF does not
         * provide case insensitive matching :( */
        var repos = GetAllRepositories();
        foreach (var repo in repos)
        {
            if (repo.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return repo;
            }
        }
        return null;
    }

    public bool IsAuditPushUser(string name)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));

        /* The straight-forward solution of using FindFirstOrDefault with
         * string.Equal does not work. Even name.Equals with OrdinalIgnoreCase does not
         * as it seems to get translated into some specific SQL syntax and EF does not
         * provide case insensitive matching :( */
        return _context.Repositories
            .SingleOrDefault(x => x.Name == name)?
            .AuditPushUser ?? false;
    }

    public RepositoryModel GetRepository(int id)
    {
        return ConvertToModel(Get(id));
    }

    private Repository Get(int id)
    {
        return _context.Repositories
            .Include(x => x.Administrators)
            .Include(x => x.Teams)
            .Include(x => x.Users)
            .AsSplitQuery()
            .First(i => i.Id.Equals(id));
    }

    public void Delete(int id)
    {
        var repo = _context.Repositories.FirstOrDefault(i => i.Id == id);
        if (repo != null)
        {
            repo.Administrators.Clear();
            repo.Users.Clear();
            repo.Teams.Clear();
            _context.Repositories.Remove(repo);
            _context.SaveChanges();
        }
    }

    public bool NameIsUnique(string newName, int ignoreRepoId)
    {
        var repo = GetRepository(newName);
        return repo == null || repo.Id == ignoreRepoId;
    }

    public string NormalizeRepositoryName(string incomingRepositoryName)
    {
        // In the most common case, we're just going to find the repo straight off
        // This is fastest if it succeeds, but might be case-sensitive
        var knownRepos = this.GetRepository(incomingRepositoryName);
        if (knownRepos != null)
        {
            return knownRepos.Name;
        }

        // We might have a real repo, but it wasn't returned by GetRepository, because that's not 
        // guaranteed to be case insensitive (very difficult to assure this with EF, because it's the back
        // end which matters, not EF itself)
        // We'll try and check all repos in a slow but safe fashion
        knownRepos =
            this.GetAllRepositories()
                .FirstOrDefault(
                    repo => repo.Name.Equals(incomingRepositoryName, StringComparison.OrdinalIgnoreCase));
        if (knownRepos != null)
        {
            // We've found it now
            return knownRepos.Name;
        }

        // We can't find this repo - it's probably invalid, but it's not
        // our job to worry about that
        return incomingRepositoryName;
    }

    public bool Create(RepositoryModel model)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));
        ArgumentNullException.ThrowIfNull(model.Name, nameof(model.Name));

        var repository = new Repository
        {
            Name = model.Name,
            Logo = model.Logo,
            Group = model.Group,
            Description = model.Description,
            Anonymous = model.AnonymousAccess,
            AllowAnonymousPush = model.AllowAnonymousPush,
            AuditPushUser = model.AuditPushUser,
            LinksUseGlobal = model.LinksUseGlobal,
            LinksUrl = model.LinksUrl,
            LinksRegex = model.LinksRegex
        };
        _context.Repositories.Add(repository);
        AddMembers(model.Users.Select(x => x.Id), model.Administrators.Select(x => x.Id), model.Teams.Select(x => x.Id), repository, _context);
        try
        {
            _context.SaveChanges();
            model.Id = repository.Id;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to create repo {RepoName}", model.Name);
            return false;
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    public void Update(RepositoryModel model)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));
        ArgumentNullException.ThrowIfNull(model.Name, nameof(model.Name));

        var repo = Get(model.Id);
        if (repo != null)
        {
            repo.Name = model.Name;
            repo.Group = model.Group;
            repo.Description = model.Description;
            repo.Anonymous = model.AnonymousAccess;
            repo.AuditPushUser = model.AuditPushUser;
            repo.AllowAnonymousPush = model.AllowAnonymousPush;
            repo.LinksRegex = model.LinksRegex;
            repo.LinksUrl = model.LinksUrl;
            repo.LinksUseGlobal = model.LinksUseGlobal;

            if (model.Logo != null)
                repo.Logo = model.Logo;

            if (model.RemoveLogo)
                repo.Logo = null;

            repo.Users.Clear();
            repo.Teams.Clear();
            repo.Administrators.Clear();

            AddMembers(model.Users.Select(x => x.Id), model.Administrators.Select(x => x.Id), model.Teams.Select(x => x.Id), repo, _context);

            _context.SaveChanges();
        }
    }

    private TeamModel TeamToTeamModel(Team t)
    {
        return new TeamModel
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            Members = t.Users.Select(user => user.ToModel()).ToArray()
        };
    }

    private RepositoryModel ConvertToModel(Repository item)
    {
        if (item == null)
        {
            return null;
        }

        return new RepositoryModel
        {
            Id = item.Id,
            Name = item.Name,
            Group = item.Group,
            Description = item.Description,
            AnonymousAccess = item.Anonymous,
            Users = item.Users.Select(user => user.ToModel()).ToArray(),
            Teams = item.Teams.Select(TeamToTeamModel).ToArray(),
            Administrators = item.Administrators.Select(user => user.ToModel()).ToArray(),
            AuditPushUser = item.AuditPushUser,
            AllowAnonymousPush = item.AllowAnonymousPush,
            Logo = item.Logo,
            LinksRegex = item.LinksRegex,
            LinksUrl = item.LinksUrl,
            LinksUseGlobal = item.LinksUseGlobal

        };
    }

    private static void AddMembers(IEnumerable<int> users, IEnumerable<int> admins, IEnumerable<int> teams, Repository repo, GibbonGitServerContext database)
    {
        if (admins != null)
        {
            var administrators = database.Users.Where(i => admins.Contains(i.Id));
            foreach (var item in administrators)
            {
                repo.Administrators.Add(item);
            }
        }

        if (users != null)
        {
            var permittedUsers = database.Users.Where(i => users.Contains(i.Id));
            foreach (var item in permittedUsers)
            {
                repo.Users.Add(item);
            }
        }

        if (teams != null)
        {
            var permittedTeams = database.Teams.Where(i => teams.Contains(i.Id));
            foreach (var item in permittedTeams)
            {
                repo.Teams.Add(item);
            }
        }
    }

    public List<RepositoryModel> GetTeamRepositories(int teamsId)
    {
        return GetAllRepositories()
            .Where(repo => repo.Teams.Any(team => teamsId == team.Id))
            .ToList();
    }
}
