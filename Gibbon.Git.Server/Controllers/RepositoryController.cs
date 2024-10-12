using System.Security.Principal;
using System.Text.RegularExpressions;

using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Extensions;
using Gibbon.Git.Server.Helpers;
using Gibbon.Git.Server.Middleware.Authorize;
using Gibbon.Git.Server.Models;
using Gibbon.Git.Server.Repositories;
using Gibbon.Git.Server.Security;
using Gibbon.Git.Server.Services;

using ICSharpCode.SharpZipLib.Zip;
using LibGit2Sharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Gibbon.Git.Server.Controllers;

public class RepositoryController(ILogger<RepositoryController> logger, ITeamService teamRepository, IRepositoryService repositoryService, IUserService userService, IRepositoryPermissionService repositoryPermissionService, IRepositorySynchronizer repositorySynchronizer, ServerSettings serverSettings, IPathResolver pathResolver, IRepositoryBrowserFactory repositoryBrowserFactory)
    : Controller
{
    private readonly ServerSettings _serverSettings = serverSettings;
    private readonly IPathResolver _pathResolver = pathResolver;
    private readonly ILogger<RepositoryController> _logger = logger;
    private readonly ITeamService _teamRepository = teamRepository;
    private readonly IRepositoryService _repositoryService = repositoryService;
    private readonly IUserService _userService = userService;
    private readonly IRepositoryPermissionService _repositoryPermissionService = repositoryPermissionService;
    private readonly IRepositorySynchronizer _repositorySynchronizer = repositorySynchronizer;
    private readonly IRepositoryBrowserFactory _repositoryBrowserFactory = repositoryBrowserFactory;

    [WebAuthorize]
    public IActionResult Index(string sortGroup = null, string searchString = null)
    {
        var firstList = GetIndexModel();
        if (!string.IsNullOrEmpty(searchString))
        {
            var search = searchString.ToLower();
            firstList = firstList.Where(a => a.Name.ToLower().Contains(search) ||
                                             (!string.IsNullOrEmpty(a.Group) && a.Group.ToLower().Contains(search)) ||
                                             (!string.IsNullOrEmpty(a.Description) && a.Description.ToLower().Contains(search)))
                .ToList();
        }

        foreach (var item in firstList)
        {
            SetGitUrls(item);
        }
        var list = firstList
            .GroupBy(x => x.Group)
            .OrderBy(x => x.Key, string.IsNullOrEmpty(sortGroup) || sortGroup.Equals("ASC"))
            .ToDictionary(x => x.Key ?? string.Empty, x => x.ToArray());

        return View(list);
    }

    [WebAuthorizeRepository(RequiresRepositoryAdministrator = true)]
    public IActionResult Edit(int id)
    {
        var model = ConvertRepositoryModel(_repositoryService.GetRepository(id), User);
        PopulateCheckboxListData(ref model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [WebAuthorizeRepository(RequiresRepositoryAdministrator = true)]
    public IActionResult Edit(RepositoryDetailModel model)
    {
        if (!ModelState.IsValid)
        {
            PopulateCheckboxListData(ref model);
            return View(model);
        }

        var currentUserIsInAdminList = model.PostedSelectedAdministrators != null && model.PostedSelectedAdministrators.Contains(User.Id());
        if (currentUserIsInAdminList || User.IsInRole(Definitions.Roles.Administrator))
        {
            var repoModel = ConvertRepositoryDetailModel(model);
            try
            {
                _repositoryService.Update(repoModel);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to update repo {RepoName}", model.Name);
                ModelState.AddModelError("Administrators", Resources.Repository_Edit_CantRemoveYourself);
                PopulateCheckboxListData(ref model);
                return View(model);
            }

            ViewBag.UpdateSuccess = true;
        }
        else
        {
            ModelState.AddModelError("Administrators", Resources.Repository_Edit_CantRemoveYourself);
            PopulateCheckboxListData(ref model);
            return View(model);
        }

        PopulateCheckboxListData(ref model);
        return RedirectToAction("Edit", new { model.Id });
    }

    // TODO In a different Action we want to rename a repository and MoveRepo should be called there

    private void MoveRepo(RepositoryModel oldRepo, RepositoryModel newRepo)
    {
        if (oldRepo.Name != newRepo.Name)
        {
            var oldPath = Path.Combine(_pathResolver.GetRepositories(), oldRepo.Name);
            var newPath = Path.Combine(_pathResolver.GetRepositories(), newRepo.Name);
            try
            {
                Directory.Move(oldPath, newPath);
            }
            catch (IOException exc)
            {
                ModelState.AddModelError("Name", exc.Message);
            }
        }
    }

    [WebAuthorize]
    public IActionResult Create()
    {
        if (!_repositoryPermissionService.HasCreatePermission(User.Id()))
        {
            return Unauthorized();
        }

        var model = new RepositoryDetailModel
        {
            Administrators = [_userService.GetUserModel(User.Id())],
        };
        PopulateCheckboxListData(ref model);
        return View(model);
    }

    [HttpPost]
    [WebAuthorize]
    [ValidateAntiForgeryToken]
    public IActionResult Create(RepositoryDetailModel model)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));
        _logger.LogInformation("Create repository {0}", model.Name);

        if (!_repositoryPermissionService.HasCreatePermission(User.Id()))
        {
            return Unauthorized();
        }

        PopulateCheckboxListData(ref model);
        model.Name = StringHelper.RemoveWhiteSpace(model.Name);

        if (!_repositoryService.NameIsUnique(model.Name, model.Id))
        {
            ModelState.AddModelError("Name", Resources.Validation_Duplicate_Name);
            return View(model);
        }

        if (string.IsNullOrEmpty(model.Name))
        {
            ModelState.AddModelError("Name", Resources.Repository_Create_NameFailure);
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var path = Path.Combine(_pathResolver.GetRepositories(), model.Name);
        if (Directory.Exists(path))
        {
            _repositoryService.Delete(model.Id);
            ModelState.AddModelError("", Resources.Repository_Create_DirectoryExists);
            return View(model);
        }

        var repoModel = ConvertRepositoryDetailModel(model);
        if (!_repositoryService.Create(repoModel))
        {
            ModelState.AddModelError("", Resources.Repository_Create_Failure);
            return View(model);
        }

        LibGit2Sharp.Repository.Init(path, true);
        TempData["CreateSuccess"] = true;
        TempData["SuccessfullyCreatedRepositoryName"] = model.Name;
        TempData["SuccessfullyCreatedRepositoryId"] = repoModel.Id;
        return RedirectToAction("Index");

    }

    [WebAuthorizeRepository(RequiresRepositoryAdministrator = true)]
    public IActionResult Delete(int id)
    {
        return View(ConvertRepositoryModel(_repositoryService.GetRepository(id), User));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [WebAuthorizeRepository(RequiresRepositoryAdministrator = true)]
    public IActionResult Delete(RepositoryDetailModel model)
    {
        if (model != null)
        {
            var repo = _repositoryService.GetRepository(model.Id);
            var path = Path.Combine(_pathResolver.GetRepositories(), repo.Name);
            if (Directory.Exists(path))
            {
                DeleteFileSystemInfo(new DirectoryInfo(path));
            }
            _repositoryService.Delete(repo.Id);
            TempData["DeleteSuccess"] = true;
        }
        return RedirectToAction("Index");
    }

    [WebAuthorizeRepository]
    public IActionResult Detail(int id)
    {
        ViewBag.ID = id;

        var model = ConvertRepositoryModel(_repositoryService.GetRepository(id), User);
        if (model != null)
        {
            model.IsCurrentUserAdministrator = _repositoryPermissionService.HasPermission(User.Id(), model.Id, RepositoryAccessLevel.Administer);
            SetGitUrls(model);
        }
        using (var browser = _repositoryBrowserFactory.Create(model.Name))
        {
            browser.BrowseTree(null, null, out var defaultReferenceName);
            RouteData.Values.Add("encodedName", defaultReferenceName);
        }

        return View(model);
    }

    /// <summary>
    /// Construct the URLs for the repository
    /// </summary>
    void SetGitUrls(RepositoryDetailModel model)
    {
        var request = HttpContext.Request;

        // Use IConfiguration to read the GitServerPath from the appsettings
        string serverAddress = null;// _configuration["GitServerPath"];

        // If GitServerPath is not defined, build it dynamically
        if (string.IsNullOrEmpty(serverAddress))
        {
            var port = request.Host.Port.HasValue && request.Host.Port != 80 && request.Host.Port != 443
                ? $":{request.Host.Port.Value}"
                : string.Empty;

            serverAddress = $"{request.Scheme}://{request.Host.Host}{port}{request.PathBase}/";
        }

        // Set the public Git URL for the repository
        model.GitUrl = string.Concat(serverAddress, model.Name, ".git");

        // Set the personal Git URL if the user is authenticated
        if (User.Identity.IsAuthenticated)
        {
            var username = Uri.EscapeDataString(User.Identity.Name);
            model.PersonalGitUrl = string.Concat(serverAddress.Replace("://", "://" + username + "@"), model.Name, ".git");
        }
    }

    [WebAuthorizeRepository]
    public IActionResult Tree(int id, string encodedName, string encodedPath)
    {
        var isApiRequest = HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        ViewBag.ID = id;
        var name = PathEncoder.Decode(encodedName);
        var path = PathEncoder.Decode(encodedPath);

        var repo = _repositoryService.GetRepository(id);

        using var browser = _repositoryBrowserFactory.Create(repo.Name);
        var files = browser.BrowseTree(name, path, out var referenceName, isApiRequest).ToList();

        var readme = files.FirstOrDefault(x => x.Name.Equals("readme.md", StringComparison.OrdinalIgnoreCase));
        var readmeTxt = string.Empty;
        if (readme != null)
        {
            var blob = browser.BrowseBlob(name, readme.Path, out _);
            readmeTxt = blob.Text;
        }
        var model = new RepositoryTreeModel
        {
            Name = repo.Name,
            Branch = name ?? referenceName,
            Path = path,
            Readme = readmeTxt,
            Logo = new RepositoryLogoDetailModel(repo.Logo),
            Files = files.OrderByDescending(i => i.IsTree).ThenBy(i => i.Name)
        };

        if (isApiRequest)
        {
            return Json(model);
        }

        PopulateBranchesData(browser, referenceName);
        PopulateAddressBarData(path);
        return View(model);
    }

    [WebAuthorizeRepository]
    public IActionResult Blob(int id, string encodedName, string encodedPath)
    {
        ViewBag.ID = id;
        var repo = _repositoryService.GetRepository(id);
        using var browser = _repositoryBrowserFactory.Create(repo.Name);
        var name = PathEncoder.Decode(encodedName);
        var path = PathEncoder.Decode(encodedPath);
        var model = browser.BrowseBlob(name, path, out var referenceName);
        model.Logo = new RepositoryLogoDetailModel(repo.Logo);
        PopulateBranchesData(browser, referenceName);
        PopulateAddressBarData(path);

        return View(model);
    }

    [WebAuthorizeRepository]
    public IActionResult Raw(int id, string encodedName, string encodedPath, bool display = false)
    {
        var repo = _repositoryService.GetRepository(id);
        using var browser = _repositoryBrowserFactory.Create(repo.Name);
        var name = PathEncoder.Decode(encodedName);
        var path = PathEncoder.Decode(encodedPath);
        var model = browser.BrowseBlob(name, path, out _);

        if (!display)
        {
            return File(model.Data, "application/octet-stream", model.Name);
        }

        if (model.IsText)
        {
            return Content(model.Text, "text/plain", model.Encoding);
        }

        if (model.IsImage)
        {
            return File(model.Data, MimeTypes.GetMimeType(model.Name), model.Name);
        }

        return NotFound();
    }

    [WebAuthorizeRepository]
    public IActionResult Blame(int id, string encodedName, string encodedPath)
    {
        ViewBag.ID = id;
        ViewBag.ShowShortMessageOnly = true;
        var repo = _repositoryService.GetRepository(id);
        using var browser = _repositoryBrowserFactory.Create(repo.Name);
        var name = PathEncoder.Decode(encodedName);
        var path = PathEncoder.Decode(encodedPath);
        var model = browser.GetBlame(name, path, out var referenceName);
        model.Logo = new RepositoryLogoDetailModel(repo.Logo);
        PopulateBranchesData(browser, referenceName);
        PopulateAddressBarData(path);

        return View(model);
    }

    [WebAuthorizeRepository]
    public IActionResult Download(int id, string encodedName, string encodedPath)
    {
        var name = PathEncoder.Decode(encodedName);
        var path = PathEncoder.Decode(encodedPath);

        var repo = _repositoryService.GetRepository(id);
        var fileName = (name ?? repo.Name) + ".zip";
        var headerValue = ContentDispositionUtil.GetHeaderValue(fileName);

        Response.ContentType = "application/zip";
        Response.Headers["Content-Disposition"] = $"attachment; filename={headerValue}";

        using var memoryStream = new MemoryStream();
        using var zipStream = new ZipOutputStream(memoryStream);
        zipStream.IsStreamOwner = false;
        zipStream.UseZip64 = UseZip64.On;

        zipStream.SetLevel(9);

        using (var browser = _repositoryBrowserFactory.Create(repo.Name))
        {
            AddTreeToZip(browser, name, path, zipStream);
        }

        zipStream.Finish();
        memoryStream.Position = 0;

        return File(memoryStream.ToArray(), "application/zip", fileName);
    }

    private static void AddTreeToZip(IRepositoryBrowser browser, string name, string path, ZipOutputStream zipStream)
    {
        var treeNode = browser.BrowseTree(name, path, out _);

        foreach (var item in treeNode)
        {
            if (item.IsLink)
            {
                var entryName = Path.Combine(item.TreeName, item.Path).Replace("\\", "/"); // Für Konsistenz in Zip-Pfaden
                var entry = new ZipEntry(entryName + "/")
                {
                    DateTime = DateTime.Now,
                    Size = 0
                };
                zipStream.PutNextEntry(entry);
                zipStream.CloseEntry();
            }
            else if (!item.IsTree)
            {
                var model = browser.BrowseBlob(item.TreeName, item.Path, out _);
                var entryName = Path.Combine(item.TreeName, item.Path).Replace("\\", "/");

                var entry = new ZipEntry(entryName)
                {
                    DateTime = DateTime.Now,
                    Size = model.Data.Length
                };

                zipStream.PutNextEntry(entry);
                zipStream.Write(model.Data, 0, model.Data.Length);
                zipStream.CloseEntry();
            }
            else
            {
                // Rekursiver Aufruf für Unterverzeichnisse
                AddTreeToZip(browser, item.TreeName, item.Path, zipStream);
            }
        }
    }

    [WebAuthorizeRepository]
    public IActionResult Tags(int id, string encodedName, int page = 1)
    {
        page = page >= 1 ? page : 1;

        ViewBag.ID = id;
        ViewBag.ShowShortMessageOnly = true;
        var repo = _repositoryService.GetRepository(id);
        using var browser = _repositoryBrowserFactory.Create(repo.Name);
        var name = PathEncoder.Decode(encodedName);
        var commits = browser.GetTags(name, page, 10, out var referenceName, out var totalCount);
        PopulateBranchesData(browser, referenceName);
        ViewBag.TotalCount = totalCount;
        return View(new RepositoryCommitsModel
        {
            Commits = commits,
            Name = repo.Name,
            Logo = new RepositoryLogoDetailModel(repo.Logo)
        });
    }

    [WebAuthorizeRepository]
    public IActionResult Commits(int id, string encodedName, int? page = null)
    {
        page = page >= 1 ? page : 1;

        ViewBag.ID = id;
        ViewBag.ShowShortMessageOnly = true;
        var repo = _repositoryService.GetRepository(id);
        using var browser = _repositoryBrowserFactory.Create(repo.Name);
        var name = PathEncoder.Decode(encodedName);
        var commits = browser.GetCommits(name, page.Value, 10, out var referenceName, out var totalCount);
        PopulateBranchesData(browser, referenceName);
        ViewBag.TotalCount = totalCount;

        var linksreg = repo.LinksUseGlobal ? _serverSettings.LinksRegex : repo.LinksRegex;
        var linksurl = repo.LinksUseGlobal ? _serverSettings.LinksUrl : repo.LinksUrl;
        foreach (var commit in commits)
        {
            var links = new List<string>();
            if (!string.IsNullOrEmpty(linksreg))
            {
                try
                {
                    var matches = Regex.Matches(commit.Message, linksreg);
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            var groups = match.Groups.Cast<Group>();
                            var link = "";
                            try
                            {
                                var m = groups.Select(x => x.ToString()).ToArray();
                                link = string.Format(linksurl, m);
                            }
                            catch (FormatException e)
                            {
                                link = "An error occured while trying to format the link. Exception: " + e.Message;
                            }
                            links.Add(link);
                        }
                    }
                }
                catch (ArgumentException e)
                {
                    links.Add("An error occured while trying to match the regualar expression. Error: " + e.Message);
                }
            }
            commit.Links = links;
        }
        return View(new RepositoryCommitsModel
        {
            Commits = commits,
            Name = repo.Name,
            Logo = new RepositoryLogoDetailModel(repo.Logo)
        });
    }

    [WebAuthorizeRepository]
    public IActionResult Commit(int id, string commit)
    {
        ViewBag.ID = id;
        ViewBag.ShowShortMessageOnly = false;
        var repo = _repositoryService.GetRepository(id);
        using var browser = _repositoryBrowserFactory.Create(repo.Name);
        var model = browser.GetCommitDetail(commit);
        model.Name = repo.Name;
        model.Logo = new RepositoryLogoDetailModel(repo.Logo);
        return View(model);
    }

    [WebAuthorize]
    public IActionResult Clone(int id)
    {
        if (!_repositoryPermissionService.HasCreatePermission(User.Id()))
        {
            return Unauthorized();
        }

        var model = ConvertRepositoryModel(_repositoryService.GetRepository(id), User);
        model.Name = "";
        PopulateCheckboxListData(ref model);
        ViewBag.ID = id;
        return View(model);
    }

    [HttpPost]
    [WebAuthorize]
    [WebAuthorizeRepository]
    [ValidateAntiForgeryToken]
    public IActionResult Clone(int id, RepositoryDetailModel model)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        if (!_repositoryPermissionService.HasCreatePermission(User.Id()))
        {
            return Unauthorized();
        }

        ViewBag.ID = id;
        PopulateCheckboxListData(ref model);
        model.Name = StringHelper.RemoveWhiteSpace(model.Name);

        if (string.IsNullOrEmpty(model.Name))
        {
            ModelState.AddModelError("Name", Resources.Repository_Create_NameFailure);
            return View(model);
        }

        if (!_repositoryService.NameIsUnique(model.Name, 0))
        {
            ModelState.AddModelError("Name", Resources.Validation_Duplicate_Name);
            return View(model);
        }

        var targetRepositoryPath = Path.Combine(_pathResolver.GetRepositories(), model.Name);
        if (Directory.Exists(targetRepositoryPath))
        {
            ModelState.AddModelError("", Resources.Repository_Create_DirectoryExists);
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var repoModel = ConvertRepositoryDetailModel(model);
        if (!_repositoryService.Create(repoModel))
        {
            ModelState.AddModelError("", Resources.Repository_Create_Failure);
            return View(model);
        }

        var sourceRepo = _repositoryService.GetRepository(id);
        var sourceRepositoryPath = Path.Combine(_pathResolver.GetRepositories(), sourceRepo.Name);

        var options = new LibGit2Sharp.CloneOptions
        {
            IsBare = true,
            Checkout = false
        };

        LibGit2Sharp.Repository.Clone(sourceRepositoryPath, targetRepositoryPath, options);

        using (var repo = new LibGit2Sharp.Repository(targetRepositoryPath))
        {
            if (repo.Network.Remotes.Any(r => r.Name == "origin"))
            {
                repo.Network.Remotes.Remove("origin");
            }
        }

        TempData["CloneSuccess"] = true;
        return RedirectToAction("Index");
    }

    [WebAuthorizeRepository]
    public IActionResult History(int id, string encodedPath, string encodedName)
    {
        ViewBag.ID = id;
        ViewBag.ShowShortMessageOnly = true;
        var repo = _repositoryService.GetRepository(id);
        using var browser = _repositoryBrowserFactory.Create(repo.Name);
        var path = PathEncoder.Decode(encodedPath);
        var name = PathEncoder.Decode(encodedName);
        var commits = browser.GetHistory(path, name, out _);
        return View(new RepositoryCommitsModel
        {
            Commits = commits,
            Name = repo.Name,
            Logo = new RepositoryLogoDetailModel(repo.Logo)
        });
    }

    private void PopulateCheckboxListData(ref RepositoryDetailModel model)
    {
        model = model.Id != 0 ? ConvertRepositoryModel(_repositoryService.GetRepository(model.Id), User) : model;
        model.AllAdministrators = _userService.GetAllUsers().ToArray();
        model.AllUsers = _userService.GetAllUsers().ToArray();
        model.AllTeams = _teamRepository.GetAllTeams().ToArray();
        if (model.PostedSelectedUsers != null && model.PostedSelectedUsers.Any())
        {
            model.Users = model.PostedSelectedUsers.Select(x => _userService.GetUserModel(x)).ToArray();
        }
        if (model.PostedSelectedTeams != null && model.PostedSelectedTeams.Any())
        {
            model.Teams = model.PostedSelectedTeams.Select(x => _teamRepository.GetTeam(x)).ToArray();
        }
        if (model.PostedSelectedAdministrators != null && model.PostedSelectedAdministrators.Any())
        {
            model.Administrators = model.PostedSelectedAdministrators.Select(x => _userService.GetUserModel(x)).ToArray();
        }
        model.PostedSelectedAdministrators = [];
        model.PostedSelectedUsers = [];
        model.PostedSelectedTeams = [];
    }

    [HttpPost]
    [WebAuthorize(Roles = Definitions.Roles.Administrator)]
    // This takes an irrelevant ID, because there isn't a good route
    // to RepositoryController for anything without an Id which isn't the Index action
    public IActionResult Rescan(string id)
    {
        _repositorySynchronizer.SynchronizeRepository();
        return RedirectToAction("Index");
    }

    private void PopulateAddressBarData(string path)
    {
        ViewData["path"] = path;
    }

    private void PopulateBranchesData(IRepositoryBrowser browser, string referenceName)
    {
        ViewData["referenceName"] = referenceName;
        ViewData["branches"] = browser.GetBranches();
        ViewData["tags"] = browser.GetTags();
    }

    private List<RepositoryDetailModel> GetIndexModel()
    {
        return _repositoryPermissionService.GetAllPermittedRepositories(User.Id(), RepositoryAccessLevel.Pull).Select(x => ConvertRepositoryModel(x, User)).ToList();
    }

    public RepositoryDetailModel ConvertRepositoryModel(RepositoryModel model, IPrincipal user)
    {
        return model == null ? null : new RepositoryDetailModel
        {
            Id = model.Id,
            Name = model.Name,
            Group = model.Group,
            Description = model.Description,
            Users = model.Users,
            Administrators = model.Administrators,
            Teams = model.Teams,
            IsCurrentUserAdministrator = model.Administrators.Select(x => x.Id).Contains(user.Id()),
            AllowAnonymous = model.AnonymousAccess,
            AllowAnonymousPush = model.AllowAnonymousPush,
            Status = GetRepositoryStatus(model),
            Logo = new RepositoryLogoDetailModel(model.Logo),
            LinksUseGlobal = model.LinksUseGlobal,
            LinksRegex = model.LinksRegex,
            LinksUrl = model.LinksUrl,
        };
    }

    private RepositoryDetailStatus GetRepositoryStatus(RepositoryModel model)
    {
        // TODO we are missing invalid here
        var repositoryPath = _pathResolver.GetRepositoryPath(model.Name);
        if (!Directory.Exists(repositoryPath))
        {
            return RepositoryDetailStatus.Missing;
        }

        if (!Repository.IsValid(repositoryPath))
        {
            return RepositoryDetailStatus.Invalid;
        }

        return RepositoryDetailStatus.Valid;
    }

    private RepositoryModel ConvertRepositoryDetailModel(RepositoryDetailModel model)
    {
        return model == null ? null : new RepositoryModel
        {
            Id = model.Id,
            Name = model.Name,
            Group = model.Group,
            Description = model.Description,
            Users = model.PostedSelectedUsers != null ? model.PostedSelectedUsers.Select(x => _userService.GetUserModel(x)).ToArray() : [],
            Administrators = model.PostedSelectedAdministrators != null ? model.PostedSelectedAdministrators.Select(x => _userService.GetUserModel(x)).ToArray() : [],
            Teams = model.PostedSelectedTeams != null ? model.PostedSelectedTeams.Select(x => _teamRepository.GetTeam(x)).ToArray() : [],
            AnonymousAccess = model.AllowAnonymous,
            Logo = model.Logo?.BinaryData,
            AllowAnonymousPush = model.AllowAnonymousPush,
            RemoveLogo = model.Logo is { RemoveLogo: true },
            LinksUseGlobal = model.LinksUseGlobal,
            LinksRegex = model.LinksRegex ?? "",
            LinksUrl = model.LinksUrl ?? ""
        };
    }

    private static void DeleteFileSystemInfo(FileSystemInfo fileSystemInfo)
    {
        fileSystemInfo.Attributes = FileAttributes.Normal;

        if (fileSystemInfo is DirectoryInfo directoryInfo)
        {
            foreach (var dirInfo in directoryInfo.GetFileSystemInfos())
            {
                DeleteFileSystemInfo(dirInfo);
            }
        }

        fileSystemInfo.Delete();
    }
}
