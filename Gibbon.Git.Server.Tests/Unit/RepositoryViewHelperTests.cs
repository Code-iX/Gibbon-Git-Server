using Gibbon.Git.Server.Data;
using Gibbon.Git.Server.Helpers;

namespace Gibbon.Git.Server.Tests.Unit;

[TestClass]
public class RepositoryViewHelperTests
{
    [TestMethod]
    [Description("Test that GetActionName returns 'Detail' for Detail view")]
    public void GetActionName_Detail_ReturnsDetail()
    {
        // Act
        var result = RepositoryViewHelper.GetActionName(RepositoryDefaultView.Detail);

        // Assert
        Assert.AreEqual("Detail", result);
    }

    [TestMethod]
    [Description("Test that GetActionName returns 'Tree' for Tree view")]
    public void GetActionName_Tree_ReturnsTree()
    {
        // Act
        var result = RepositoryViewHelper.GetActionName(RepositoryDefaultView.Tree);

        // Assert
        Assert.AreEqual("Tree", result);
    }

    [TestMethod]
    [Description("Test that GetActionName returns 'Commits' for Commits view")]
    public void GetActionName_Commits_ReturnsCommits()
    {
        // Act
        var result = RepositoryViewHelper.GetActionName(RepositoryDefaultView.Commits);

        // Assert
        Assert.AreEqual("Commits", result);
    }

    [TestMethod]
    [Description("Test that GetActionName returns 'Tags' for Tags view")]
    public void GetActionName_Tags_ReturnsTags()
    {
        // Act
        var result = RepositoryViewHelper.GetActionName(RepositoryDefaultView.Tags);

        // Assert
        Assert.AreEqual("Tags", result);
    }

    [TestMethod]
    [Description("Test that GetActionName returns 'Detail' for invalid/unknown view value")]
    public void GetActionName_InvalidValue_ReturnsDetail()
    {
        // Arrange - Cast an invalid enum value
        var invalidView = (RepositoryDefaultView)999;

        // Act
        var result = RepositoryViewHelper.GetActionName(invalidView);

        // Assert
        Assert.AreEqual("Detail", result);
    }
}
