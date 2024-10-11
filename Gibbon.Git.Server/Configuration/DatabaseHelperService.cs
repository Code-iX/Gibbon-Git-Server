using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace Gibbon.Git.Server.Configuration;

public class DatabaseHelperService(GibbonGitServerContext context, IPathResolver pathResolver) : IDatabaseHelperService
{
    private readonly GibbonGitServerContext _context = context;
    private readonly IPathResolver _pathResolver = pathResolver;

    public DatabaseInformation GetDatabaseInformation()
    {
        var dataSource = _context.Database
            .GetDbConnection()
            .DataSource;

        var databasePath = _pathResolver.ResolveRoot(dataSource);
        var databaseSize = new FileInfo(databasePath).Length;

        return new DatabaseInformation
        {
            Path = databasePath,
            Size = databaseSize
        };
    }
}
