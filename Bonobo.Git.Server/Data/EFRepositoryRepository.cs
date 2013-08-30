﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bonobo.Git.Server.Models;
using System.Data;
using Bonobo.Git.Server.Data;

namespace Bonobo.Git.Server.Data
{
    public class EFRepositoryRepository : IRepositoryRepository
    {
        public IList<Models.RepositoryModel> GetAllRepositories()
        {
            using (var db = new BonoboGitServerContext())
            {
                var result = new List<RepositoryModel>();
                foreach (var item in db.Repositories)
                {
                    result.Add(ConvertToModel(item));
                }
                return result;
            }
        }

        public IList<RepositoryModel> GetPermittedRepositories(string username, string[] teams)
        {
            if (username == null) throw new ArgumentException("username");

            username = username.ToLowerInvariant();
            return GetAllRepositories().Where(i => i.Administrators.Contains(username)
                || i.Users.Contains(username)
                || i.Teams.FirstOrDefault(t => teams.Contains(t)) != null
                || i.AnonymousAccess).ToList();
        }

        public IList<RepositoryModel> GetAdministratedRepositories(string username)
        {
            if (username == null) throw new ArgumentException("username");
            
            username = username.ToLowerInvariant();
            return GetAllRepositories().Where(i => i.Administrators.Contains(username)).ToList();
        }

        public RepositoryModel GetRepository(string name)
        {
            if (name == null) throw new ArgumentException("name");

            using (var db = new BonoboGitServerContext())
            {
                return ConvertToModel(db.Repositories.FirstOrDefault(i => i.Name == name));
            }
        }

        public void Delete(string name)
        {
            if (name == null) throw new ArgumentException("name");

            using (var db = new BonoboGitServerContext())
            {
                var repo = db.Repositories.FirstOrDefault(i => i.Name == name);
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

        public bool Create(RepositoryModel model)
        {
            if (model == null) throw new ArgumentException("model");
            if (model.Name == null) throw new ArgumentException("name");

            using (var database = new BonoboGitServerContext())
            {
                var repository = new Repository
                {
                    Name = model.Name,
                    Description = model.Description,
                    Anonymous = model.AnonymousAccess,
                };
                database.Repositories.Add(repository);
                AddMembers(model.Users, model.Administrators, model.Teams, repository, database);
                try
                {
                    database.SaveChanges();
                }
                catch (UpdateException)
                {
                    return false;
                }
            }

            return true;
        }

        public void Update(RepositoryModel model)
        {
            if (model == null) throw new ArgumentException("model");
            if (model.Name == null) throw new ArgumentException("name");

            using (var db = new BonoboGitServerContext())
            {
                var repo = db.Repositories.FirstOrDefault(i => i.Name == model.Name);
                if (repo != null)
                {
                    repo.Description = model.Description;
                    repo.Anonymous = model.AnonymousAccess;

                    repo.Users.Clear();
                    repo.Teams.Clear();
                    repo.Administrators.Clear();

                    AddMembers(model.Users, model.Administrators, model.Teams, repo, db);

                    db.SaveChanges();
                }
            }
        }

        private RepositoryModel ConvertToModel(Repository item)
        {
            if (item != null)
            {
                return new RepositoryModel
                        {
                            Name = item.Name,
                            Description = item.Description,
                            AnonymousAccess = item.Anonymous,
                            Users = item.Users.Select(i => i.Username).ToArray(),
                            Teams = item.Teams.Select(i => i.Name).ToArray(),
                            Administrators = item.Administrators.Select(i => i.Username).ToArray(),
                        };
            }

            return null;
        }

        private void AddMembers(string[] users, string[] admins, string[] teams, Repository repo, BonoboGitServerContext database)
        {
            if (admins != null)
            {
                var administrators = database.Users.Where(i => admins.Contains(i.Username));
                foreach (var item in administrators)
                {
                    repo.Administrators.Add(item);
                }
            }

            if (users != null)
            {
                var permittedUsers = database.Users.Where(i => users.Contains(i.Username));
                foreach (var item in permittedUsers)
                {
                    repo.Users.Add(item);
                }
            }

            if (teams != null)
            {
                var permittedTeams = database.Teams.Where(i => teams.Contains(i.Name));
                foreach (var item in permittedTeams)
                {
                    repo.Teams.Add(item);
                }
            }
        }

    }
}