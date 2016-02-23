﻿using System;
using System.Collections.Generic;
using System.Linq;
using Bonobo.Git.Server.Models;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using Microsoft.Practices.Unity;

namespace Bonobo.Git.Server.Data
{
    public class EFRepositoryRepository : RepositoryRepositoryBase
    {
        [Dependency]
        public Func<BonoboGitServerContext> CreateContext { get; set; }

        public override IList<RepositoryModel> GetAllRepositories()
        {
            using (var db = CreateContext())
            {
                var dbrepos = db.Repositories.Select(repo => new
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
                    Logo = repo.Logo
                }).ToList();
            }
        }

        public override RepositoryModel GetRepository(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            using (var db = CreateContext())
            {
                return ConvertToModel(db.Repositories.FirstOrDefault(i => i.Name == name));
            }
        }

        public override RepositoryModel GetRepository(Guid id)
        {
            using (var db = CreateContext())
            {
                return ConvertToModel(db.Repositories.First(i => i.Id.Equals(id)));
            }
        }

        public override void Delete(Guid id)
        {
            using (var db = CreateContext())
            {
                var repo = db.Repositories.FirstOrDefault(i => i.Id == id);
                if (repo != null)
                {
                    repo.Administrators.Clear();
                    repo.Users.Clear();
                    repo.Teams.Clear();
                    db.Repositories.Remove(repo);
                    db.SaveChanges();
                }
            }
        }

        public override bool Create(RepositoryModel model)
        {
            if (model == null) throw new ArgumentException("model");
            if (model.Name == null) throw new ArgumentException("name");

            using (var database = CreateContext())
            {
                model.Id = Guid.NewGuid();
                var repository = new Repository
                {
                    Id = model.Id,
                    Name = model.Name,
                    Logo = model.Logo,
                    Group = model.Group,
                    Description = model.Description,
                    Anonymous = model.AnonymousAccess,
                    AuditPushUser = model.AuditPushUser,
                };
                database.Repositories.Add(repository);
                AddMembers(model.Users.Select(x => x.Id), model.Administrators.Select(x => x.Id), model.Teams.Select(x => x.Id), repository, database);
                try
                {
                    database.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    return false;
                }
                catch (UpdateException)
                {
                    return false;
                }
                return true;
            }
        }

        public override void Update(RepositoryModel model)
        {
            if (model == null) throw new ArgumentException("model");
            if (model.Name == null) throw new ArgumentException("name");

            using (var db = CreateContext())
            {
                var repo = db.Repositories.FirstOrDefault(i => i.Id == model.Id);
                if (repo != null)
                {
                    repo.Name = model.Name;
                    repo.Group = model.Group;
                    repo.Description = model.Description;
                    repo.Anonymous = model.AnonymousAccess;
                    repo.AuditPushUser = model.AuditPushUser;

                    if (model.Logo != null)
                        repo.Logo = model.Logo;

                    if (model.RemoveLogo)
                        repo.Logo = null;

                    repo.Users.Clear();
                    repo.Teams.Clear();
                    repo.Administrators.Clear();

                    AddMembers(model.Users.Select(x => x.Id), model.Administrators.Select(x => x.Id), model.Teams.Select(x => x.Id), repo, db);

                    db.SaveChanges();
                }
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
                Logo = item.Logo
            };
        }

        private void AddMembers(IEnumerable<Guid> users, IEnumerable<Guid> admins, IEnumerable<Guid> teams, Repository repo, BonoboGitServerContext database)
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

    }
}