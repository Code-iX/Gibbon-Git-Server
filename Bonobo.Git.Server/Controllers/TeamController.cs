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
            var model = ConvertTeamModel(TeamRepository.GetTeam(id));
            PopulateViewData();
            return View(model);
        }

        [HttpPost]
        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Edit(TeamDetailModel model)
        {           
            if (ModelState.IsValid)
            {
                TeamRepository.Update(ConvertTeamDetailModel(model));
                ViewBag.UpdateSuccess = true;
            }
            PopulateViewData();
            return View(model);
        }

        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Create()
        {
            var model = new TeamDetailModel { };
            PopulateViewData();
            return View(model);
        }

        [HttpPost]
        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Create(TeamDetailModel model)
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

            PopulateViewData();
            return View(model);
        }

        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Delete(int id)
        {
            return View(ConvertTeamModel(TeamRepository.GetTeam(id)));
        }

        [HttpPost]
        [WebAuthorize(Roles = Definitions.Roles.Administrator)]
        public ActionResult Delete(TeamDetailModel model)
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
            return View(ConvertTeamModel(TeamRepository.GetTeam(id)));
        }


        private TeamDetailModelList ConvertTeamModels(IEnumerable<TeamModel> models)
        {
            var result = new TeamDetailModelList();
            result.IsReadOnly = MembershipService.IsReadOnly();
            foreach (var item in models)
            {
                result.Add(ConvertTeamModel(item));
            }
            return result;
        }

        private TeamDetailModel ConvertTeamModel(TeamModel model)
        {
            return model == null ? null : new TeamDetailModel
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                Members = model.Members,
                Repositories = RepositoryRepository.GetPermittedRepositories(null, new[] { model.Name }).Select(x => x.Name).ToArray(),
                IsReadOnly = MembershipService.IsReadOnly()
            };
        }

        private TeamModel ConvertTeamDetailModel(TeamDetailModel model)
        {
            return new TeamModel
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                Members = model.Members,
            };
        }

        private void PopulateViewData()
        {
            ViewData["AvailableUsers"] = MembershipService.GetAllUsers().Select(i => i.Name).ToArray();
        }
    }
}
