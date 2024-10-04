using System.Threading.Tasks;

namespace Gibbon.Git.Server.Services;

public interface IStartupService
{
    Task RunAsync();
}
