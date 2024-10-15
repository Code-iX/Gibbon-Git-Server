using System.Linq.Expressions;

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
        return _context.Repositories
            .AsNoTracking()
            .Include(repo => repo.Administrators)
            .Include(repo => repo.Teams)
            .Include(repo => repo.Users)
            .AsSplitQuery()
            .Select(repo => new RepositoryModel
            {
                Id = repo.Id,
                Name = repo.Name,
                Group = repo.Group,
                Description = repo.Description,
                AnonymousAccess = repo.Anonymous,
                Users = repo.Users.Select(user => new UserModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    GivenName = user.GivenName,
                    Surname = user.Surname,
                    Email = user.Email
                }).ToArray(),
                Teams = repo.Teams.Select(team => new TeamModel
                {
                    Id = team.Id,
                    Name = team.Name,
                    Description = team.Description,
                    Members = team.Users.Select(user => new UserModel
                    {
                        Id = user.Id,
                        Username = user.Username,
                        GivenName = user.GivenName,
                        Surname = user.Surname,
                        Email = user.Email
                    }).ToArray()
                }).ToArray(),
                Administrators = repo.Administrators.Select(admin => new UserModel
                {
                    Id = admin.Id,
                    Username = admin.Username,
                    GivenName = admin.GivenName,
                    Surname = admin.Surname,
                    Email = admin.Email
                }).ToArray(),
                AllowAnonymousPush = repo.AllowAnonymousPush,
                Logo = repo.Logo,
                LinksRegex = repo.LinksRegex,
                LinksUrl = repo.LinksUrl,
                LinksUseGlobal = repo.LinksUseGlobal
            }).ToList();
    }

    public RepositoryModel GetRepository(string name)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));

        return GetRepositoryInternal(repo => repo.Name == name);
    }

    public RepositoryModel GetRepository(int id)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(id, 0, nameof(id));

        return GetRepositoryInternal(repo => repo.Id == id) ?? throw new InvalidOperationException("Repository not found");
    }

    private RepositoryModel GetRepositoryInternal(Expression<Func<Repository, bool>> predicate)
    {
        return _context.Repositories
            .AsNoTracking()
            .Include(repo => repo.Administrators)
            .Include(repo => repo.Teams)
                .ThenInclude(team => team.Users)
            .Include(repo => repo.Users)
            .AsSplitQuery()
            .Where(predicate)
            .Select(repo => new RepositoryModel
            {
                Id = repo.Id,
                Name = repo.Name,
                Group = repo.Group,
                Description = repo.Description,
                AnonymousAccess = repo.Anonymous,
                Users = repo.Users.Select(user => new UserModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    GivenName = user.GivenName,
                    Surname = user.Surname,
                    Email = user.Email
                }).ToArray(),
                Teams = repo.Teams.Select(team => new TeamModel
                {
                    Id = team.Id,
                    Name = team.Name,
                    Description = team.Description,
                    Members = team.Users.Select(user => new UserModel
                    {
                        Id = user.Id,
                        Username = user.Username,
                        GivenName = user.GivenName,
                        Surname = user.Surname,
                        Email = user.Email
                    }).ToArray()
                }).ToArray(),
                Administrators = repo.Administrators.Select(admin => new UserModel
                {
                    Id = admin.Id,
                    Username = admin.Username,
                    GivenName = admin.GivenName,
                    Surname = admin.Surname,
                    Email = admin.Email
                }).ToArray(),
                AllowAnonymousPush = repo.AllowAnonymousPush,
                Logo = repo.Logo,
                LinksRegex = repo.LinksRegex,
                LinksUrl = repo.LinksUrl,
                LinksUseGlobal = repo.LinksUseGlobal
            })
            .SingleOrDefault(); // TODO is this necessare?
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

    public string NormalizeRepositoryName(string repositoryName)
    {
        var repository = _context.Repositories
            .AsNoTracking()
            .Select(x => x.Name)
            .FirstOrDefault(name => name == repositoryName);

        return repository ?? repositoryName;
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

        var repo = _context.Repositories
            .Include(x => x.Administrators)
            .Include(x => x.Teams)
            .Include(x => x.Users)
            .AsSplitQuery()
            .First(i => i.Id.Equals(model.Id));
        if (repo != null)
        {
            repo.Group = model.Group;
            repo.Description = model.Description;
            repo.Anonymous = model.AnonymousAccess;
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
