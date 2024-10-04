using System.ComponentModel.DataAnnotations;
using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Middleware.Attributes;
using Gibbon.Git.Server.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Gibbon.Git.Server.Controllers;

[OutputCache(NoStore = true)]
public class ValidationController(IRepositoryService repoRepo, IMembershipService membershipService, ITeamService teamRepo)
    : Controller
{
    public IRepositoryService RepoRepo { get; set; } = repoRepo;

    public IMembershipService MembershipService { get; set; } = membershipService;

    public ITeamService TeamRepo { get; set; } = teamRepo;

    [AcceptVerbs("GET", "POST")]
    public IActionResult UniqueNameRepo(string name, Guid? id)
    {
        var isUnique = RepoRepo.NameIsUnique(name, id ?? Guid.Empty);
        return Json(isUnique);
    }

    public IActionResult UniqueNameUser(string username, Guid? id)
    {
        var possiblyExistentUser = MembershipService.GetUserModel(username);
        var exists = (possiblyExistentUser != null) && (id != possiblyExistentUser.Id);
        return Json(!exists);
    }

    public IActionResult UniqueNameTeam(string name, Guid? id)
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
