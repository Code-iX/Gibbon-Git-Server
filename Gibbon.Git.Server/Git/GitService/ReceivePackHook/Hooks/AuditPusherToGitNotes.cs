using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Security;
using Gibbon.Git.Server.Services;
using LibGit2Sharp;
using Repository = LibGit2Sharp.Repository;

namespace Gibbon.Git.Server.Git.GitService.ReceivePackHook.Hooks;

public class AuditPusherToGitNotes(IHookReceivePack hookReceivePack, IPathResolver pathResolver, IRepositoryService repositoryService, IMembershipService membershipService)
    : IHookReceivePack
{
    public const string EmptyUserName = "anonymous";

    private readonly IPathResolver _pathResolver = pathResolver;
    private readonly IHookReceivePack _hookReceivePack = hookReceivePack;
    private readonly IRepositoryService _repositoryService = repositoryService;
    private readonly IMembershipService _membershipService = membershipService;

    public void PrePackReceive(ParsedReceivePack receivePack)
    {
        _hookReceivePack.PrePackReceive(receivePack);
    }

    public void PostPackReceive(ParsedReceivePack receivePack, GitExecutionResult result)
    {
        _hookReceivePack.PostPackReceive(receivePack, result);

        if (result.HasError)
        {
            return;
        }

        if (!_repositoryService.IsAuditPushUser(receivePack.RepositoryName))
        {
            return;
        }

        var user = receivePack.PushedByUser;
        var email = string.Empty;

        if (string.IsNullOrEmpty(user))
        {
            user = EmptyUserName;
        }
        else
        {
            var userData = _membershipService.GetUserModel(user);
            if (userData != null)
            {
                email = userData.Email;
            }
        }

        var gitRepo = new Repository(_pathResolver.GetRepositoryPath(receivePack.RepositoryName));
        foreach (var commit in receivePack.Commits)
        {
            gitRepo.Notes.Add(
                new ObjectId(commit.Id),
                user,
                new Signature(user, email, DateTimeOffset.Now),
                new Signature(user, email, DateTimeOffset.Now),
                "pusher");
        }
    }
}
