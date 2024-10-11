using System.ComponentModel.DataAnnotations;
using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Middleware.Attributes;
using Gibbon.Git.Server.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Gibbon.Git.Server.Controllers;

[OutputCache(NoStore = true)]
public class ValidationController(IRepositoryService repoRepo, IUserService userService, ITeamService teamRepo)
    : Controller
{
    public IRepositoryService RepoRepo { get; set; } = repoRepo;

    public IUserService UserService { get; set; } = userService;

    public ITeamService TeamRepo { get; set; } = teamRepo;

    [AcceptVerbs("GET", "POST")]
    public IActionResult UniqueNameRepo(string name, int? id)
    {
        var isUnique = RepoRepo.NameIsUnique(name, id ?? 0);
        return Json(isUnique);
    }

    public IActionResult UniqueNameUser(string username, int? id)
    {
        var possiblyExistentUser = UserService.GetUserModel(username);
        var exists = (possiblyExistentUser != null) && (id != possiblyExistentUser.Id);
        return Json(!exists);
    }

    public IActionResult UniqueNameTeam(string name, int? id)
    {
        var result = TeamRepo.IsTeamNameUnique(name, id);
        return Json(result);
    }

    public IActionResult IsValidRegex(string linksRegex)
    {
        var validationContext = new ValidationContext(new { });
        var isValidRegexAttr = new IsValidRegexAttribute();
        var result = isValidRegexAttr.GetValidationResult(linksRegex, validationContext);

        if (result == System.ComponentModel.DataAnnotations.ValidationResult.Success)
        {
            return Json(true);
        }

        return Json(result.ErrorMessage);
    }
}
