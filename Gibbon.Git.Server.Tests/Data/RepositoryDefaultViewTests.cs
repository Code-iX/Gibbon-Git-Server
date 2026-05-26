using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Gibbon.Git.Server.Data;

namespace Gibbon.Git.Server.Tests.Data;

[TestClass]
public class RepositoryDefaultViewTests
{
    [TestMethod]
    [Description("Test that Detail enum value is 0")]
    public void RepositoryDefaultView_Detail_IsZero()
    {
        // Assert
        Assert.AreEqual(0, (int)RepositoryDefaultView.Detail);
    }

    [TestMethod]
    [Description("Test that Tree enum value is 1")]
    public void RepositoryDefaultView_Tree_IsOne()
    {
        // Assert
        Assert.AreEqual(1, (int)RepositoryDefaultView.Tree);
    }

    [TestMethod]
    [Description("Test that Commits enum value is 2")]
    public void RepositoryDefaultView_Commits_IsTwo()
    {
        // Assert
        Assert.AreEqual(2, (int)RepositoryDefaultView.Commits);
    }

    [TestMethod]
    [Description("Test that Tags enum value is 3")]
    public void RepositoryDefaultView_Tags_IsThree()
    {
        // Assert
        Assert.AreEqual(3, (int)RepositoryDefaultView.Tags);
    }

    [TestMethod]
    [Description("Test that Detail has Display attribute")]
    public void RepositoryDefaultView_Detail_HasDisplayAttribute()
    {
        // Arrange
        var fieldInfo = typeof(RepositoryDefaultView).GetField(nameof(RepositoryDefaultView.Detail));

        // Act
        var displayAttribute = fieldInfo?.GetCustomAttribute<DisplayAttribute>();

        // Assert
        Assert.IsNotNull(displayAttribute);
        Assert.IsNotNull(displayAttribute.ResourceType);
        Assert.AreEqual("Repository_Detail_Detail", displayAttribute.Name);
    }

    [TestMethod]
    [Description("Test that Tree has Display attribute")]
    public void RepositoryDefaultView_Tree_HasDisplayAttribute()
    {
        // Arrange
        var fieldInfo = typeof(RepositoryDefaultView).GetField(nameof(RepositoryDefaultView.Tree));

        // Act
        var displayAttribute = fieldInfo?.GetCustomAttribute<DisplayAttribute>();

        // Assert
        Assert.IsNotNull(displayAttribute);
        Assert.IsNotNull(displayAttribute.ResourceType);
        Assert.AreEqual("Repository_Layout_Browse", displayAttribute.Name);
    }

    [TestMethod]
    [Description("Test that Commits has Display attribute")]
    public void RepositoryDefaultView_Commits_HasDisplayAttribute()
    {
        // Arrange
        var fieldInfo = typeof(RepositoryDefaultView).GetField(nameof(RepositoryDefaultView.Commits));

        // Act
        var displayAttribute = fieldInfo?.GetCustomAttribute<DisplayAttribute>();

        // Assert
        Assert.IsNotNull(displayAttribute);
        Assert.IsNotNull(displayAttribute.ResourceType);
        Assert.AreEqual("Repository_Layout_Commits", displayAttribute.Name);
    }

    [TestMethod]
    [Description("Test that Tags has Display attribute")]
    public void RepositoryDefaultView_Tags_HasDisplayAttribute()
    {
        // Arrange
        var fieldInfo = typeof(RepositoryDefaultView).GetField(nameof(RepositoryDefaultView.Tags));

        // Act
        var displayAttribute = fieldInfo?.GetCustomAttribute<DisplayAttribute>();

        // Assert
        Assert.IsNotNull(displayAttribute);
        Assert.IsNotNull(displayAttribute.ResourceType);
        Assert.AreEqual("Repository_Layout_Tags", displayAttribute.Name);
    }

    [TestMethod]
    [Description("Test that all enum values have Display attributes")]
    public void RepositoryDefaultView_AllValuesHaveDisplayAttributes()
    {
        // Arrange
        var enumValues = Enum.GetValues(typeof(RepositoryDefaultView));

        // Act & Assert
        foreach (RepositoryDefaultView value in enumValues)
        {
            var fieldInfo = typeof(RepositoryDefaultView).GetField(value.ToString());
            var displayAttribute = fieldInfo?.GetCustomAttribute<DisplayAttribute>();

            Assert.IsNotNull(displayAttribute, $"{value} should have a Display attribute");
            Assert.IsNotNull(displayAttribute.ResourceType, $"{value} should have a ResourceType");
            Assert.IsNotNull(displayAttribute.Name, $"{value} should have a Name");
        }
    }
}
