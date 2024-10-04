using Gibbon.Git.Server.Models;

namespace Gibbon.Git.Server.Tests.Models;

[TestClass]
public class RepositoryModelTests
{
    [DataTestMethod]
    [DataRow("Test", true)]
    [DataRow("Valid-Name123", true)]
    [DataRow("another.valid.name", true)]
    [DataRow("name_with.periods.and-dashes", true)]
    [DataRow("name_with.periods.and-dashes.", false)]
    [DataRow("@name_with.periods.and-dashes", false)]
    [DataRow("", false)]
    [DataRow("Test!", false)]
    [DataRow("Invalid@Name", false)]
    [DataRow(" ", false)]
    [DataRow("Test Name", false)]
    [DataRow(" TestName", false)]
    public void NameIsValid_TestVariousNames(string name, bool expectedResult)
    {
        // Arrange
        var repositoryModel = new RepositoryModel
        {
            Name = name
        };

        // Act
        var result = repositoryModel.NameIsValid;

        // Assert
        Assert.AreEqual(expectedResult, result);
    }
}
