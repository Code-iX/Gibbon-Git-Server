﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Bonobo.Git.Server.App_GlobalResources;

using Microsoft.Practices.Unity;

namespace Bonobo.Git.Server.Controllers
{
    public class TeamController : Controller
    {
        [Dependency]
        public IMembershipService MembershipService { get; set; }

        [Dependency]
        public IRepositoryRepository RepositoryRepository { get; set; }

        [Dependency]
        public ITeamRepository TeamRepository { get; set; }

        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Index()
        {
            return View(ConvertTeamModels(TeamRepository.GetAllTeams()));
        }

        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Edit(int id)
        {
            var model = ConvertEditTeamModel(TeamRepository.GetTeam(id));
            return View(model);
        }

        [HttpPost]
        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Edit(TeamEditModel model)
        {           
            if (ModelState.IsValid)
            {
                TeamRepository.Update(ConvertTeamDetailModel(model));
                ViewBag.UpdateSuccess = true;
            }
            model = ConvertEditTeamModel(TeamRepository.GetTeam(model.Id));
            return View(model);
        }

        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Create()
        {
            var model = new TeamEditModel 
            {
                AllUsers = MembershipService.GetAllUsers().ToArray(),
                SelectedUsers = new UserModel[] { }
            };
            return View(model);
        }

        [HttpPost]
        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Create(TeamEditModel model)
        {
            while (!String.IsNullOrEmpty(model.Name) && model.Name.Last() == ' ')
            {
                model.Name = model.Name.Substring(0, model.Name.Length - 1);
            }

            if (ModelState.IsValid)
            {
                if (TeamRepository.Create(ConvertTeamDetailModel(model)))
                {
                    TempData["CreateSuccess"] = true;
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", Resources.Team_Create_Failure);
                }
            }

            return View(model);
        }

        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Delete(int id)
        {
            return View(ConvertEditTeamModel(TeamRepository.GetTeam(id)));
        }

        [HttpPost]
        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Delete(TeamEditModel model)
        {
            if (model != null && model.Id != 0)
            {
                TeamModel team = TeamRepository.GetTeam(model.Id);
                TeamRepository.Delete(team.Name);
                TempData["DeleteSuccess"] = true;
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        [WebAuthorize]
        public ActionResult Detail(int id)
        {
            return View(ConvertDetailTeamModel(TeamRepository.GetTeam(id)));
        }


        private TeamDetailModelList ConvertTeamModels(IEnumerable<TeamModel> models)
        {
            var result = new TeamDetailModelList();
            result.IsReadOnly = MembershipService.IsReadOnly();
            foreach (var item in models)
            {
                result.Add(ConvertDetailTeamModel(item));
            }
            return result;
        }

        private TeamEditModel ConvertEditTeamModel(TeamModel model)
        {
            return model == null ? null : new TeamEditModel
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                AllUsers = MembershipService.GetAllUsers().ToArray(),
                SelectedUsers = model.Members.ToArray(),
            };
        }

        private TeamDetailModel ConvertDetailTeamModel(TeamModel model)
        {
            return model == null ? null : new TeamDetailModel
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                Members = model.Members.ToArray(),
                Repositories = RepositoryRepository.GetPermittedRepositories(null, new[] { model.Name }).ToArray(),
                IsReadOnly = MembershipService.IsReadOnly()
            };
        }

        private TeamModel ConvertTeamDetailModel(TeamEditModel model)
        {
            return new TeamModel
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                Members = model.PostedSelectedUsers.Select(x => MembershipService.GetUser(int.Parse(x))).ToArray(),
            };
        }
    }
}
