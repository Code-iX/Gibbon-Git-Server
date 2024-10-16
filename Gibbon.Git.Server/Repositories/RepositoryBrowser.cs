using System.Diagnostics;

using Gibbon.Git.Server.Helpers;
using Gibbon.Git.Server.Models;
using Gibbon.Git.Server.Services;

using LibGit2Sharp;

using Microsoft.Extensions.Logging;

namespace Gibbon.Git.Server.Repositories;

public sealed class RepositoryBrowser(IAvatarService avatarService, ILogger<RepositoryBrowser> logger, IPathResolver pathResolver) : IRepositoryBrowser
{
    private readonly ILogger<RepositoryBrowser> _logger = logger;
    private readonly IPathResolver _pathResolver = pathResolver;
    private readonly IAvatarService _avatarService = avatarService;
    private Repository _repository;

    public void SetRepository(string name)
    {
        Debug.Assert(!string.IsNullOrEmpty(name), "Repository path is null or empty.");
        if (_repository != null)
        {
            throw new InvalidOperationException("Repository is already set.");
        }

        var repositoryPath = _pathResolver.GetRepositoryPath(name);

        if (!Repository.IsValid(repositoryPath))
        {
            _logger.LogError("Invalid repo path {RespositoryPath}", repositoryPath);
            throw new ArgumentException("Repository is not valid.", nameof(repositoryPath));
        }
        _repository = new Repository(repositoryPath);
    }

    public List<string> GetBranches()
    {
        return _repository.Branches.Select(s => s.FriendlyName).ToList();
    }

    public List<string> GetTags()
    {
        return _repository.Tags.Select(s => s.FriendlyName).OrderByDescending(s => s).ToList();
    }

    public List<RepositoryCommitModel> GetCommits(string name, int page, int pageSize, out string referenceName, out int totalCount)
    {
        var commit = GetCommitByName(name, out referenceName);
        if (commit == null)
        {
            totalCount = 0;
            return [];
        }

        IEnumerable<Commit> commitLogQuery = _repository.Commits
            .QueryBy(new CommitFilter { IncludeReachableFrom = commit, SortBy = CommitSortStrategies.Topological });

        totalCount = commitLogQuery.Count();

        if (page >= 1 && pageSize >= 1)
        {
            commitLogQuery = commitLogQuery.Skip((page - 1) * pageSize).Take(pageSize);
        }

        return commitLogQuery.Select(s => ToModel(s)).ToList();
    }

    public List<RepositoryCommitModel> GetTags(string name, int page, int p, out string referenceName, out int totalCount)
    {
        var commit = GetCommitByName(name, out referenceName);
        if (commit == null)
        {
            totalCount = 0;
            return [];
        }
        var tags = _repository.Tags;
        var commits = new HashSet<RepositoryCommitModel>(InlineComparer<RepositoryCommitModel>.Create((x, y) => x.ID == y.ID, obj => obj.ID.GetHashCode()));
        foreach (var tag in tags)
        {
            var c = _repository.Lookup(tag.Target.Id) as Commit;
            commits.Add(ToModel(c));

        }
        totalCount = commits.Count;

        return commits.OrderByDescending(x => x.Date).ToList();
    }

    public RepositoryCommitModel GetCommitDetail(string name)
    {
        var commit = GetCommitByName(name, out _);
        return commit == null ? null : ToModel(commit, true);
    }

    public List<RepositoryTreeDetailModel> BrowseTree(string name, string path, out string referenceName, bool includeDetails = false)
    {
        var commit = GetCommitByName(name, out referenceName);
        if (commit == null)
        {
            return [];
        }

        var branchName = referenceName ?? name;

        Tree tree;
        if (string.IsNullOrEmpty(path))
        {
            tree = commit.Tree;
        }
        else
        {
            var treeEntry = commit[path];
            if (treeEntry.TargetType == TreeEntryTargetType.Blob)
            {
                return [CreateRepositoryDetailModel(treeEntry, null, referenceName)];
            }

            if (treeEntry.TargetType == TreeEntryTargetType.GitLink)
            {
                return [];
            }

            tree = (Tree)treeEntry.Target;
        }

        var result = includeDetails ? GetTreeModelsWithDetails(commit, tree, branchName) : GetTreeModels(tree, branchName);
        return result.ToList();
    }

    public RepositoryTreeDetailModel BrowseBlob(string name, string path, out string referenceName)
    {
        if (path == null)
        {
            path = string.Empty;
        }

        var commit = GetCommitByName(name, out referenceName);
        if (commit == null)
        {
            return null;
        }

        var entry = commit[path];
        if (entry == null)
        {
            return null;
        }

        var model = new RepositoryTreeDetailModel
        {
            Name = entry.Name,
            IsTree = false,
            IsLink = false,
            CommitDate = commit.Author.When.LocalDateTime,
            CommitMessage = commit.Message,
            Author = commit.Author.Name,
            TreeName = referenceName ?? name,
            Path = path,
        };

        using (var memoryStream = new MemoryStream())
        {
            ((Blob)entry.Target).GetContentStream().CopyTo(memoryStream);
            model.Data = memoryStream.ToArray();
        }

        if (FileDisplayHandler.TryGetEncoding(model.Data, out var encoding))
        {
            model.Text = FileDisplayHandler.GetText(model.Data, encoding);
            model.Encoding = encoding;
            model.IsText = model.Text != null;
            model.IsMarkdown = model.IsText && Path.GetExtension(path).Equals(".md", StringComparison.OrdinalIgnoreCase);
        }
        model.TextBrush = FileDisplayHandler.GetBrush(path);

        // try to render as text file if the extension matches
        if (model.TextBrush != FileDisplayHandler.NoBrush && !model.IsText)
        {
            model.IsText = true;
            model.Encoding = Encoding.Default;
            model.Text = new StreamReader(new MemoryStream(model.Data), model.Encoding, true).ReadToEnd();
        }

        //blobs can be images even when they are text files.(like svg, but it's not in out MIME table yet)
        model.IsImage = FileDisplayHandler.IsImage(path);

        return model;
    }

    public RepositoryBlameModel GetBlame(string name, string path, out string referenceName)
    {
        var modelBlob = BrowseBlob(name, path, out referenceName);
        if (modelBlob == null || !modelBlob.IsText)
        {
            return null;
        }
        var commit = GetCommitByName(name, out referenceName);
        var lines = modelBlob.Text.Split(["\r\n", "\n"], StringSplitOptions.None);
        var hunks = new List<RepositoryBlameHunkModel>();
        foreach (var hunk in _repository.Blame(path, new BlameOptions { StartingAt = commit }))
        {
            hunks.Add(new RepositoryBlameHunkModel
            {
                Commit = ToModel(hunk.FinalCommit),
                Lines = lines.Skip(hunk.FinalStartLineNumber).Take(hunk.LineCount).ToArray()
            });
        }
        return new RepositoryBlameModel
        {
            Name = commit[path].Name,
            TreeName = referenceName,
            Path = path,
            Hunks = hunks,
            FileSize = modelBlob.Data.LongLength,
            LineCount = lines.LongLength
        };
    }

    public List<RepositoryCommitModel> GetHistory(string path, string name, out string referenceName)
    {
        var commit = GetCommitByName(name, out referenceName);
        if (commit == null || string.IsNullOrEmpty(path))
        {
            return [];
        }

        return _repository.Commits
            .QueryBy(new CommitFilter { IncludeReachableFrom = commit, SortBy = CommitSortStrategies.Topological })
            .Where(c => c.Parents.Count() < 2 && c[path] != null && (!c.Parents.Any() || c.Parents.FirstOrDefault()[path] == null || c[path].Target.Id != c.Parents.FirstOrDefault()[path].Target.Id))
            .Select(s => ToModel(s)).ToList();
    }

    public void Dispose()
    {
        _repository?.Dispose();
    }

    private List<RepositoryTreeDetailModel> GetTreeModelsWithDetails(Commit commit, IEnumerable<TreeEntry> tree, string referenceName)
    {
        var ancestors = _repository.Commits.QueryBy(new CommitFilter { IncludeReachableFrom = commit, SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Reverse }).ToList();
        var entries = tree.ToList();
        var result = new List<RepositoryTreeDetailModel>();

        for (var i = 0; i < ancestors.Count && entries.Any(); i++)
        {
            var ancestor = ancestors[i];

            for (var j = 0; j < entries.Count; j++)
            {
                var entry = entries[j];
                var ancestorEntry = ancestor[entry.Path];
                if (ancestorEntry != null && entry.Target.Sha == ancestorEntry.Target.Sha)
                {
                    result.Add(CreateRepositoryDetailModel(entry, ancestor, referenceName));
                    entries[j] = null;
                }
            }

            entries.RemoveAll(entry => entry == null);
        }

        return result;
    }

    private List<RepositoryTreeDetailModel> GetTreeModels(IEnumerable<TreeEntry> tree, string referenceName)
    {
        return tree.Select(i => CreateRepositoryDetailModel(i, null, referenceName)).ToList();
    }

    private RepositoryTreeDetailModel CreateRepositoryDetailModel(TreeEntry entry, Commit ancestor, string treeName)
    {
        var maximumMessageLength = 50; //TODO Probably in appSettings?
        var originMessage = ancestor != null ? ancestor.Message : string.Empty;
        var commitMessage = !string.IsNullOrEmpty(originMessage)
            ? RepositoryCommitModelHelpers.MakeCommitMessage(originMessage, maximumMessageLength).ShortTitle : string.Empty;

        return new RepositoryTreeDetailModel
        {
            Name = entry.Name,
            CommitDate = ancestor != null ? ancestor.Author.When.LocalDateTime : default(DateTime?),
            CommitMessage = commitMessage,
            Author = ancestor != null ? ancestor.Author.Name : string.Empty,
            IsTree = entry.TargetType == TreeEntryTargetType.Tree,
            IsLink = entry.TargetType == TreeEntryTargetType.GitLink,
            TreeName = treeName,
            Path = entry.Path.Replace('\\', '/'),
            IsImage = FileDisplayHandler.IsImage(entry.Name),
        };
    }

    private Commit GetCommitByName(string name, out string referenceName)
    {
        referenceName = null;

        if (string.IsNullOrEmpty(name))
        {
            referenceName = _repository.Head.FriendlyName;
            return _repository.Head.Tip;
        }

        var repositoryBranch = _repository.Branches[name];
        if (repositoryBranch != null && repositoryBranch.Tip != null)
        {
            referenceName = repositoryBranch.FriendlyName;
            return repositoryBranch.Tip;
        }

        var tag = _repository.Tags[name];
        if (tag == null)
        {
            return _repository.Lookup(name) as Commit;
        }

        referenceName = tag.FriendlyName;
        return tag.Target as Commit;
    }

    private RepositoryCommitModel ToModel(Commit commit, bool withDiff = false)//, Tuple<bool, string, string> linkify)
    {
        var tags = _repository.Tags.Where(o => o.Target.Sha == commit.Sha).Select(o => o.FriendlyName).ToList();

        var shortMessageDetails = RepositoryCommitModelHelpers.MakeCommitMessage(commit.Message, 50);

        var model = new RepositoryCommitModel
        {
            Author = commit.Author.Name,
            AuthorEmail = commit.Author.Email,
            AuthorAvatar = _avatarService.GetAvatar(commit.Author.Email),
            Date = commit.Author.When.LocalDateTime,
            ID = commit.Sha,
            Message = shortMessageDetails.ShortTitle,
            MessageShort = shortMessageDetails.ExtraTitle,
            TreeID = commit.Tree.Sha,
            Parents = commit.Parents.Select(i => i.Sha).ToArray(),
            Tags = tags,
            Notes = (from n in commit.Notes select new RepositoryCommitNoteModel(n.Message, n.Namespace)).ToList()
        };

        if (!withDiff)
        {
            return model;
        }

        var changes = _repository.Diff.Compare<TreeChanges>(commit.Parents.FirstOrDefault()?.Tree, commit.Tree);
        var patches = _repository.Diff.Compare<Patch>(commit.Parents.FirstOrDefault()?.Tree, commit.Tree);

        model.Changes = changes.OrderBy(s => s.Path).Select(i =>
        {
            var patch = patches[i.Path];
            return new RepositoryCommitChangeModel
            {
                ChangeId = i.Oid.Sha,
                Path = i.Path.Replace('\\', '/'),
                Status = i.Status,
                LinesAdded = patch.LinesAdded,
                LinesDeleted = patch.LinesDeleted,
                Patch = patch.Patch,

            };
        });

        return model;
    }
}
