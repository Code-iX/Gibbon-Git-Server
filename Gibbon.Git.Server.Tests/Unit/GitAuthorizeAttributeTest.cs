using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Services;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

using NSubstitute;

namespace Gibbon.Git.Server.Tests.Unit;

[TestClass]
public class PathResolverTests
{
    private IPathResolver _pathResolver = null!;

    [TestInitialize]
    public void Initialize()
    {
        IWebHostEnvironment webEnvironment = Substitute.For<IWebHostEnvironment>();
        webEnvironment.ContentRootPath.Returns("C:\\");

        IOptions<ApplicationSettings> configuration = Substitute.For<IOptions<ApplicationSettings>>();
        configuration.Value.Returns(new ApplicationSettings
        {
            RepositoryPath = "test",
            DataPath = "~\\Data"
        });
        _pathResolver = new PathResolver(webEnvironment, configuration);
    }

    [TestMethod]
    public void GetRepoPathTest()
    {
        var repo = _pathResolver.GetRepoPath("/other/test.git/info/refs", "/other");
        Assert.AreEqual("test", repo);
        repo = _pathResolver.GetRepoPath("/test.git/info/refs", "/");
        Assert.AreEqual("test", repo);
    }

    [TestMethod]
    public void ResolveTest()
    {
        var path = _pathResolver.Resolve("test");
        Assert.AreEqual("C:\\Data\\test", path);
    }

    [TestMethod]
    public void ResolveTest_TwoPaths()
    {
    }
}
