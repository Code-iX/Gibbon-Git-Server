using System.ComponentModel.DataAnnotations;
using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Middleware.Attributes;
using Gibbon.Git.Server.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Gibbon.Git.Server.Controllers;

[Route("Validation/[action]")]
[OutputCache(NoStore = true)]
public class ValidationController
    : Controller
{
    [AcceptVerbs("GET", "POST")]
    public IActionResult UniqueNameRepo(string name, int? id, [FromServices] IRepositoryService repositoryService)
    {
        var isUnique = repositoryService.NameIsUnique(name, id ?? 0);
        return Json(isUnique);
    }

    public IActionResult UniqueNameUser(string username, int? id, [FromServices] IUserService userService)
    {
        var possiblyExistentUser = userService.GetUserModel(username);
        var exists = (possiblyExistentUser != null) && (id != possiblyExistentUser.Id);
        return Json(!exists);
    }

    public IActionResult UniqueNameTeam(string name, int? id, [FromServices] ITeamService teamService)
    {
        var result = teamService.IsTeamNameUnique(name, id);
        return Json(result);
    }

    public IActionResult IsValidRegex(string linksRegex)
    {
        var validationContext = new ValidationContext(new { });
        var isValidRegexAttr = new IsValidRegexAttribute();
        var result = isValidRegexAttr.GetValidationResult(linksRegex, validationContext);

        if (result == ValidationResult.Success)
        {
            return Json(true);
        }

        return Json(result.ErrorMessage);
    }
}
