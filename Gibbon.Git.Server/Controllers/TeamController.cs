using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Middleware.Authorize;
using Gibbon.Git.Server.Models;
using Gibbon.Git.Server.Security;

using Microsoft.AspNetCore.Mvc;

namespace Gibbon.Git.Server.Controllers;

public class TeamController(IUserService userService, IRepositoryService repositoryService, ITeamService teamRepository)
    : Controller
{
    private readonly IUserService _userService = userService;
    private readonly IRepositoryService _repositoryService = repositoryService;
    private readonly ITeamService _teamRepository = teamRepository;

    [WebAuthorize(Roles = Definitions.Roles.Administrator)]
    public IActionResult Index()
    {
        return View(ConvertTeamModels(_teamRepository.GetAllTeams()));
    }

    [WebAuthorize(Roles = Definitions.Roles.Administrator)]
    public IActionResult Edit(int id)
    {
        var model = ConvertEditTeamModel(_teamRepository.GetTeam(id));
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [WebAuthorize(Roles = Definitions.Roles.Administrator)]
    public IActionResult Edit(TeamEditModel model)
    {
        if (ModelState.IsValid)
        {
            TeamModel detailModel = ConvertTeamDetailModel(model);
            _teamRepository.Update(detailModel);
            ViewBag.UpdateSuccess = true;
        }
        model = ConvertEditTeamModel(_teamRepository.GetTeam(model.Id));
        return RedirectToAction("Edit");
    }

    [WebAuthorize(Roles = Definitions.Roles.Administrator)]
    public IActionResult Create()
    {
        var model = new TeamEditModel
        {
            AllUsers = _userService.GetAllUsers().ToArray(),
            SelectedUsers = new UserModel[] { }
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [WebAuthorize(Roles = Definitions.Roles.Administrator)]
    public IActionResult Create(TeamEditModel model)
    {
        while (!string.IsNullOrEmpty(model.Name) && model.Name.Last() == ' ')
        {
            model.Name = model.Name[..^1];
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var teammodel = ConvertTeamDetailModel(model);
        if (!_teamRepository.Create(teammodel))
        {
            ModelState.AddModelError("", Resources.Team_Create_Failure);
            return View(model);
        }

        TempData["CreateSuccess"] = true;
        TempData["NewTeamId"] = teammodel.Id;
        return RedirectToAction("Index");
    }

    [WebAuthorize(Roles = Definitions.Roles.Administrator)]
    public IActionResult Delete(int id)
    {
        return View(ConvertEditTeamModel(_teamRepository.GetTeam(id)));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [WebAuthorize(Roles = Definitions.Roles.Administrator)]
    public IActionResult Delete(TeamEditModel model)
    {
        if (model != null && model.Id != default)
        {
            var team = _teamRepository.GetTeam(model.Id);
            _teamRepository.Delete(team.Id);
            TempData["DeleteSuccess"] = true;
            return RedirectToAction("Index");
        }
        return RedirectToAction("Index");
    }

    [WebAuthorize]
    public IActionResult Detail(int id)
    {
        return View(ConvertDetailTeamModel(_teamRepository.GetTeam(id)));
    }

    private List<TeamDetailModel> ConvertTeamModels(IEnumerable<TeamModel> models)
    {
        var result = new List<TeamDetailModel>();
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
            AllUsers = _userService.GetAllUsers().ToArray(),
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
            Repositories = _repositoryService.GetTeamRepositories(model.Id).ToArray(),
        };
    }

    private TeamModel ConvertTeamDetailModel(TeamEditModel model)
    {
        return new TeamModel
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Members = model.PostedSelectedUsers == null ? [] : model.PostedSelectedUsers.Select(x => _userService.GetUserModel(x)).ToArray(),
        };
    }
}
