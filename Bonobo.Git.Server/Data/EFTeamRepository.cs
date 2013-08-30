﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Data;

namespace Bonobo.Git.Server.Data
{
    public class EFTeamRepository : ITeamRepository
    {
        public IList<TeamModel> GetAllTeams()
        {
            using (var db = new BonoboGitServerContext())
            {
                var result = new List<TeamModel>();
                foreach (var item in db.Teams)
                {
                    result.Add(new TeamModel
                    {
                        Name = item.Name,
                        Description = item.Description,
                        Members = item.Users.Select(i => i.Username).ToArray(),
                        Repositories = item.Repositories.Select(m => m.Name).ToArray(),
                    });
                }

                return result;
            }
        }

        public IList<TeamModel> GetTeams(string username)
        {
            username = username.ToLowerInvariant(); 
            return GetAllTeams().Where(i => i.Members.Contains(username)).ToList();
        }

        public TeamModel GetTeam(string name)
        {
            if (name == null) throw new ArgumentException("name");

            using (var db = new BonoboGitServerContext())
            {
                var team = db.Teams.FirstOrDefault(i => i.Name == name);
                return team == null ? null : new TeamModel
                {
                    Name = team.Name,
                    Description = team.Description,
                    Members = team.Users.Select(m => m.Username).ToArray(),
                    Repositories = team.Repositories.Select(m => m.Name).ToArray(),
                };
            }
        }

        public void Delete(string name)
        {
            if (name == null) throw new ArgumentException("name");

            using (var db = new BonoboGitServerContext())
            {
                var team = db.Teams.FirstOrDefault(i => i.Name == name);
                if (team != null)
                {
                    team.Repositories.Clear();
                    team.Users.Clear();
                    db.Teams.Remove(team);
                    db.SaveChanges();
                }
            }
        }

        public bool Create(TeamModel model)
        {
            if (model == null) throw new ArgumentException("team");
            if (model.Name == null) throw new ArgumentException("name");

            using (var database = new BonoboGitServerContext())
            {
                var team = new Team
                {
                    Name = model.Name,
                    Description = model.Description
                };
                database.Teams.Add(team);
                if (model.Members != null)
                {
                    AddMembers(model.Members, team, database);
                }
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

        public void Update(TeamModel model)
        {
            if (model == null) throw new ArgumentException("team");
            if (model.Name == null) throw new ArgumentException("name");

            using (var db = new BonoboGitServerContext())
            {
                var team = db.Teams.FirstOrDefault(i => i.Name == model.Name);
                if (team != null)
                {
                    team.Description = model.Description;
                    team.Users.Clear();
                    if (model.Members != null)
                    {
                        AddMembers(model.Members, team, db);
                    }
                    db.SaveChanges();
                }
            }
        }

        private void AddMembers(string[] members, Team team, BonoboGitServerContext database)
        {
            var users = database.Users.Where(i => members.Contains(i.Username));
            foreach (var item in users)
            {
                team.Users.Add(item);
            }
        }
    }
}