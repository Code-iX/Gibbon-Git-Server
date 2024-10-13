namespace Gibbon.Git.Server.Tests.Repositories;

//[TestClass]
//public class RepositoryBrowserTests : TestBase
//{
//    private IRepositoryBrowser _repositoryBrowser = null!;
//    private IPathResolver _pathResolver = null!;

//    protected override void ConfigureServices(ServiceCollection services)
//    {
//        services.AddSubstitute<IAvatarService>();
//        services.AddSubstitute<ILogger<RepositoryBrowser>>();
//        _pathResolver = services.AddSubstitute<IPathResolver>();

//        services.AddScoped<IRepositoryBrowser, RepositoryBrowser>();
//        services.AddScoped<IRepositoryBrowserFactory, RepositoryBrowserFactory>();
//    }

//    protected override void UseServices(IServiceProvider serviceProvider)
//    {
//        var factory = serviceProvider.GetRequiredService<IRepositoryBrowserFactory>();
//        _repositoryBrowser = factory.Create("test");
//    }

//    [TestMethod]
//    public void SetRepository_ShouldThrowArgumentException_WhenRepositoryPathIsInvalid()
//    {
//        // Arrange
//        var invalidRepositoryName = "invalid-repo";
//        _pathResolver.GetRepositoryPath(invalidRepositoryName).Returns("invalid/path/to/repository");
//        Repository.IsValid(Arg.Any<string>()).Returns(false);

//        // Act & Assert
//        Assert.ThrowsException<ArgumentException>(() => _repositoryBrowser.SetRepository(invalidRepositoryName));
//    }

//    [TestMethod]
//    public void SetRepository_ShouldThrowInvalidOperationException_WhenRepositoryAlreadySet()
//    {
//        // Arrange
//        var repositoryName = "valid-repo";
//        _pathResolver.GetRepositoryPath(repositoryName).Returns("valid/path/to/repository");
//        Repository.IsValid(Arg.Any<string>()).Returns(true);

//        _repositoryBrowser.SetRepository(repositoryName);

//        // Act & Assert
//        Assert.ThrowsException<InvalidOperationException>(() => _repositoryBrowser.SetRepository(repositoryName));
//    }

//    [TestMethod]
//    public void SetRepository_ShouldSetRepository_WhenRepositoryPathIsValid()
//    {
//        // Arrange
//        var validRepositoryName = "valid-repo";
//        _pathResolver.GetRepositoryPath(validRepositoryName).Returns("valid/path/to/repository");
//        Repository.IsValid(Arg.Any<string>()).Returns(true);

//        // Act
//        _repositoryBrowser.SetRepository(validRepositoryName);

//        // Assert
//    }

//    [TestMethod]
//    public void GetBranches_ShouldReturnBranchList_WhenRepositoryIsSet()
//    {
//        // Arrange
//        var branches = new[] { "main", "develop", "feature" };
//        _repositoryBrowser.GetBranches().Returns(branches.ToList());

//        // Act
//        var result = _repositoryBrowser.GetBranches();

//        // Assert
//        Assert.IsNotNull(result);
//        CollectionAssert.AreEqual(branches.ToList(), result);
//    }

//    [TestMethod]
//    public void GetTags_ShouldReturnTagList_WhenRepositoryIsSet()
//    {
//        // Arrange
//        var tags = new[] { "v1.0", "v1.1", "v2.0" };
//        _repositoryBrowser.GetTags().Returns(tags.ToList());

//        // Act
//        var result = _repositoryBrowser.GetTags();

//        // Assert
//        Assert.IsNotNull(result);
//        CollectionAssert.AreEqual(tags.ToList(), result);
//    }
//}
