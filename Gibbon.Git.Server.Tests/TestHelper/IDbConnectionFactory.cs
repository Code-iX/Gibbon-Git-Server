using Microsoft.EntityFrameworkCore;

namespace Gibbon.Git.Server.Tests.TestHelper;

public interface IDbConnectionFactory
{
    void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder);
    void Cleanup();
}
