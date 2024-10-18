using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Gibbon.Git.Server.Tests.TestHelper;

public interface IDbConnectionFactory
{
    void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder);
    void Cleanup();
    void ConfigureService(ServiceCollection services);
}
